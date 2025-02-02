using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Scripting.Customer;
using Scripting.PDollarGestureRecognizer;
using Scripting.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripting.Desk
{
    public class Contract : MonoBehaviour
    {
        [SerializeField] private float samplingRate;
        [SerializeField] private Transform downPoint;
        [SerializeField] private Transform upPoint;
        [SerializeField] private List<Material> materials;
        [SerializeField] private GameObject surface;
        [SerializeField] private AudioSource myWritingSound;
        [SerializeField] private AudioSource myHellIdleSound;
        [SerializeField] private AudioSource myHellActiveSound;

        [SerializeField] private LayerMask layerMask;
        [SerializeField] private LayerMask writingSurfaceMask;
        [SerializeField] private MeshRenderer meshRenderer;

        private readonly Gesture[] _trainingsSet = TrainingsData.GetTrainingsData();

        private bool _isActive;
        private float _lastSnapshot;

        private LineRenderer _lineRenderer;
        private int _nextCount;

        private PlayerInput _playerInput;

        public string Result { get; private set; }
        public bool Converted { get; private set; }

        private GameObject _surface;

        public void SetActive(bool value)
        {
            _isActive = value;
        }

        private Camera _cam;

        private readonly List<LineRenderer> _renderers = new();
        private readonly List<Vector3[]> _positions = new();

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void AlignWithContract()
        {
            for (var j = 0; j < _renderers.Count; j++)
            {
                var dings = _renderers[j];
                for (var i = 0; i < dings.positionCount; i++)
                {
                    var worldPos = dings.transform.TransformPoint(_positions[j][i]);

                    dings.SetPosition(i, worldPos);
                }
            }
        }

        private bool _isWriting;
        
        private void Update()
        {
            AlignWithContract();

            if (!_isActive || Converted) return;

            var position = Input.mousePosition;
            var x = _cam.ScreenPointToRay(position);

            if (Physics.Raycast(x, out var hit, Mathf.Infinity, layerMask))
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    myWritingSound.Play();
                    var camPos = _cam.transform.position;
                    var delta = (camPos - transform.position).normalized * 0.05f;

                    _surface = Instantiate(surface, transform.position + delta, transform.rotation, transform);
                    _lineRenderer = _surface.GetComponent<LineRenderer>();
                    _isWriting = true;

                    _nextCount = 0;
                    
                    _lastSnapshot = samplingRate;
                    
                    _lineRenderer.positionCount++;
                    _lineRenderer.SetPosition(_nextCount++, hit.point);
                }
            }
            else
            {
                if (_isWriting)
                {
                    FinalizeWriting();
                    _isWriting = false;
                }
            }

            if (Physics.Raycast(x, out var hit3, Mathf.Infinity, writingSurfaceMask))
            {
                if (Mouse.current.leftButton.isPressed && _isWriting)
                {
                    _lastSnapshot -= Time.deltaTime;
                    if (_lastSnapshot <= 0.0f)
                    {
                        _lastSnapshot = samplingRate;

                        _lineRenderer.positionCount++;
                        _lineRenderer.SetPosition(_nextCount++, hit3.point);
                    }
                }
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame && _isWriting)
            {
                FinalizeWriting();
            }
        }

        private void Cancel()
        { 
                _surface.GetComponent<BoxCollider>().enabled = false;
                _surface = null;
                _lineRenderer = null;
                Destroy(_surface); 
        }

        private void FinalizeWriting()
        {
            _isWriting = false;
            myWritingSound.Stop();

            if (!_lineRenderer) return;

            if (_lineRenderer.positionCount < 2)
            {
                _surface.GetComponent<BoxCollider>().enabled = false;
                _surface = null;
                _lineRenderer = null;
                Destroy(_surface);
            
                return;
            }

            var points = new List<Point>();

            var transformPos = transform.position;

            var posList = new List<Vector3>();

            for (var i = 0; i < _lineRenderer.positionCount; i++)
            {
                posList.Add(_surface.transform.InverseTransformPoint(_lineRenderer.GetPosition(i)));
            }

            _positions.Add(posList.ToArray());
            _renderers.Add(_lineRenderer);

            var counter = 0;
            for (var j = 0; j < _renderers.Count; j++)
            {
                var lineRenderer = _renderers[j];
                for (var i = 0; i < lineRenderer.positionCount - 1; i++)
                {
                    var p1 = _positions[j][i] - transformPos;
                    var p2 = _positions[j][i + 1] - transformPos;

                    points.Add(new Point(p1.x, p1.z, counter));
                    points.Add(new Point(p2.x, p2.z, counter));

                    counter++;
                }
            }

            points = Normalize(points);
            // Debug.Log(string.Join(',',
            //     points.Select(n =>
            //         $"new({n.X.ToString(CultureInfo.InvariantCulture)}f,{n.Y.ToString(CultureInfo.InvariantCulture)}f,{n.StrokeID})")));

            var gesture = PointCloudRecognizer.Classify(new Gesture(points.ToArray()), _trainingsSet);

            Result = gesture;

            _surface.GetComponent<BoxCollider>().enabled = false;
            _surface = null;
            _lineRenderer = null;

            if (!_tutorialWrite)
            {
                TutorialManager.Singleton.ShowOrderNumber(4, true);
                _tutorialWrite = true;
            }
        }

        private bool _tutorialWrite;

        public List<Point> Normalize(List<Point> points)
        {
            if (points == null || points.Count == 0) return points;

            // Step 1: Find min and max values for x and y
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var point in points)
            {
                if (point.X < minX) minX = point.X;
                if (point.Y < minY) minY = point.Y;

                if (point.X > maxX) maxX = point.X;
                if (point.Y > maxY) maxY = point.Y;
            }

            // Step 2: Normalize each point
            var rangeX = maxX - minX;
            var rangeY = maxY - minY;

            var normalizedPoints = new List<Point>();
            foreach (var point in points)
            {
                var normalizedX = (point.X - minX) / rangeX;
                var normalizedY = (point.Y - minY) / rangeY;

                // Keep the lineId unchanged
                normalizedPoints.Add(new Point(normalizedX, normalizedY, point.StrokeID));
            }

            return normalizedPoints;
        }

        public static void ResetTutorial()
        {
            _mailTutorial = false;
            _customerContractTutorial = false;
            _notRecognized = false;
        }

        private static bool _mailTutorial;
        private static bool _customerContractTutorial;
        private static bool _notRecognized;

        public void Submit()
        {
            if (!Converted)
            {
                var names = TrainingsData.ContractTypes;
                if (Result == null)
                {
                    Debug.Log("No gesture found!");
                }
                else
                {
                    if (GetIsPowerContract())
                    {
                        var gameManager = GameManager.Singleton;
                        var player = gameManager.Player;
                        switch (Result)
                        {
                            case "pentagramm":
                                if (!gameManager.upgrades.hellishContract)
                                {
                                    player.ResetContract();
                                    if (_notRecognized)
                                    {
                                        _notRecognized = true;
                                        TutorialManager.Singleton.ShowOrderNumber(10, true);
                                    }

                                    return;
                                }

                                myHellIdleSound.Play();
                                myHellActiveSound.Play();

                                break;
                            case "ds":
                                if (!GameManager.Singleton.upgrades.dismissal)
                                {
                                    player.ResetContract();
                                    if (_notRecognized)
                                    {
                                        _notRecognized = true;
                                        TutorialManager.Singleton.ShowOrderNumber(10, true);
                                    }

                                    return;
                                }

                                break;
                            case "pfr":
                                if (!GameManager.Singleton.upgrades.powerFistRequisition)
                                {
                                    player.ResetContract();
                                    if (_notRecognized)
                                    {
                                        _notRecognized = true;
                                        TutorialManager.Singleton.ShowOrderNumber(10, true);
                                    }

                                    return;
                                }

                                break;
                            case "la":
                                if (!GameManager.Singleton.upgrades.loanAgreement)
                                {
                                    player.ResetContract();
                                    if (_notRecognized)
                                    {
                                        _notRecognized = true;
                                        TutorialManager.Singleton.ShowOrderNumber(10, true);
                                    }

                                    return;
                                }

                                break;
                            case "tec":
                                if (!GameManager.Singleton.upgrades.temporaryEmploymentContract)
                                {
                                    player.ResetContract();
                                    if (_notRecognized)
                                    {
                                        _notRecognized = true;
                                        TutorialManager.Singleton.ShowOrderNumber(10, true);
                                    }

                                    return;
                                }

                                break;
                            case "eel":
                                if (!GameManager.Singleton.upgrades.endOfLifePlan)
                                {
                                    player.ResetContract();
                                    if (_notRecognized)
                                    {
                                        _notRecognized = true;
                                        TutorialManager.Singleton.ShowOrderNumber(10, true);
                                    }

                                    return;
                                }

                                break;
                        }
                    }

                    var mailbox = Result is "ds" or "eel" or "la" or "tec";

                    if (!_mailTutorial && mailbox)
                    {
                        TutorialManager.Singleton.ShowOrderNumber(7, true);
                        _mailTutorial = true;
                    }

                    if (!_customerContractTutorial && !mailbox)
                    {
                        TutorialManager.Singleton.ShowOrderNumber(new[] {5, 9}, true);
                        _customerContractTutorial = true;
                    }

                    for (var i = 0; i < names.Count; i++)
                    {
                        if (Result == names[i])
                        {
                            Converted = true;
                            meshRenderer.material = materials[i];

                            _renderers.ToList().ForEach(n => Destroy(n.gameObject));

                            _renderers.Clear();
                            _positions.Clear();
                        }
                    }
                }
            }
        }

        public void Reset()
        {
            Destroy(transform.parent.gameObject);
        }

        public bool GetIsMailBoxContract() => Converted && Result is "ds" or "eel" or "la" or "tec";

        public void ExecuteMailboxEffect(Movement player)
        {
            switch (Result)
            {
                case "ds":
                    GameManager.Singleton.Dismissal();
                    player.SetStressLevel(0);
                    break;
                case "la":
                    GameManager.Singleton.ActivateLoanAgreement();
                    break;
                case "tec":
                    GameManager.Singleton.SpawnTec();
                    break;
                case "eel":
                    GameManager.Singleton.GetExtraLife();
                    break;
            }
        }

        // "returns true or false if was power effect"
        public bool ExecuteEffect(CustomerMotor customer, Movement player)
        {
            player.ChangeStressLevel();

            if (GetIsPowerContract())
            {
                customer.Pay();
                switch (Result)
                {
                    case "pentagramm":
                        GameManager.Singleton.ChangeMoneyAmount(GameManager.Singleton.pentagramReward +
                                                                Random.Range(-1000, 1000));
                        GameManager.Singleton.DoPentagrammLogic();
                        customer.StopHuell();
                        GameManager.Singleton.RemoveCustomer(customer);
                        customer.gameObject.GetComponent<LaunchDetection>().GetHellGrabbed();
                        break;
                    case "pfr":
                        GameManager.Singleton.DoFistStuff(customer.IsMotherfucker);
                        customer.StopHuell();
                        GameManager.Singleton.RemoveCustomer(customer);
                        customer.gameObject.GetComponent<LaunchDetection>().GetPowerFisted();
                        break;
                }

                return true;
            }

            GameManager.Singleton.RemoveCustomer(customer);
            customer.Pay();
            customer.WalkOut();

            return false;
        }

        public bool GetIsPowerContract() =>
            Result is "pentagramm" or "ds" or "pfr" or "la" or "tec" or "eel";
    }
}
