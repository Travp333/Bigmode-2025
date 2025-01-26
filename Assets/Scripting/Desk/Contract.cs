using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Scripting.PDollarGestureRecognizer;
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

        [SerializeField] private LayerMask layerMask;
        [SerializeField] private MeshRenderer meshRenderer;

        private readonly Gesture[] _trainingsSet = TrainingsData.GetTrainingsData();

        private bool _isActive;
        private float _lastSnapshot;

        private LineRenderer _lineRenderer;
        private int _nextCount;

        private PlayerInput _playerInput;

        private bool _isUp;

        public bool IsUp => _isUp;
        public string Result { get; private set; }
        public bool Converted { get; private set; }
        
        private GameObject _surface;
        private const float AnimationDuration = 1.0f;

        public void SetActive(bool value)
        {
            _isActive = value;
            if (!_isActive)
            {
                StartCoroutine(DoDownSyndromeAnimation());
                _isUp = false;
            }
        }

        private Camera _cam;

        private readonly List<LineRenderer> _renderers = new();
        private readonly List<Vector3[]> _positions = new();

        void Awake()
        {
            _playerInput = new PlayerInput();

            _playerInput.Game.Action.performed += ActionPerformed;

            _cam = Camera.main;
        }

        public void Block()
        {
            _isUp = false;
            _isBlocked = true;
            StartCoroutine(DoDownSyndromeAnimation());
        }

        private bool _isBlocked;

        public void Unblock()
        {
            _isBlocked = false;
        }

        private void ActionPerformed(InputAction.CallbackContext ctx)
        {
            if (!_isActive || Mouse.current.leftButton.isPressed || _isBlocked) return;

            _isUp = !_isUp;
            StartCoroutine(_isUp ? DoUpAnimation() : DoDownSyndromeAnimation());
        }

        private IEnumerator DoUpAnimation()
        {
            var elapsed = 0f;

            while (elapsed < AnimationDuration && _isUp)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / AnimationDuration;

                transform.position = Vector3.Slerp(transform.position, upPoint.transform.position, t);
                transform.rotation = Quaternion.Slerp(transform.rotation, upPoint.transform.rotation, t);

                yield return null;
            }

            transform.position = upPoint.transform.position;
            transform.rotation = upPoint.transform.rotation;
        }

        private IEnumerator DoDownSyndromeAnimation()
        {
            var elapsed = 0f;

            while (elapsed < AnimationDuration && !_isUp)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / AnimationDuration;

                transform.position = Vector3.Slerp(transform.position, downPoint.transform.position, t);
                transform.rotation = Quaternion.Slerp(transform.rotation, downPoint.transform.rotation, t);

                yield return null;
            }

            transform.position = downPoint.transform.position;
            transform.rotation = downPoint.transform.rotation;
        }

        private void OnEnable()
        {
            _playerInput?.Enable();
        }

        private void OnDisable()
        {
            _playerInput?.Disable();
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

        void Update()
        {
            AlignWithContract();

            if (!_isActive || !_isUp || Converted) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                var camPos = _cam.transform.position;
                var delta = (camPos - transform.position).normalized * 0.05f;

                _surface = Instantiate(surface, transform.position + delta, transform.rotation, transform);
                _lineRenderer = _surface.GetComponent<LineRenderer>();
            }

            if (Mouse.current.leftButton.isPressed)
            {
                _lastSnapshot -= Time.deltaTime;
                if (_lastSnapshot <= 0.0f)
                {
                    var position = Input.mousePosition;

                    var x = _cam.ScreenPointToRay(position);
                    if (Physics.Raycast(x, out var hit, Mathf.Infinity, layerMask))
                    {
                        _lastSnapshot = samplingRate;

                        _lineRenderer.positionCount++;
                        _lineRenderer.SetPosition(_nextCount++, hit.point);
                    }
                }
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _nextCount = 0;

                if (!_lineRenderer) return;

                if (_lineRenderer.positionCount < 4)
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
                Debug.Log(string.Join(',',
                    points.Select(n =>
                        $"new({n.X.ToString(CultureInfo.InvariantCulture)}f,{n.Y.ToString(CultureInfo.InvariantCulture)}f,{n.StrokeID})")));

                var gesture = PointCloudRecognizer.Classify(new Gesture(points.ToArray()), _trainingsSet);

                Result = gesture;

                _surface.GetComponent<BoxCollider>().enabled = false;
                _surface = null;
                _lineRenderer = null;
            }
        }

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
                    Debug.Log(Result);
                    for (var i = 0; i < names.Count; i++)
                    {
                        if (Result == names[i])
                        {
                            Converted = true;
                            meshRenderer.material = materials[i];

                            _renderers.ToList().ForEach(n => { Destroy(n.gameObject); });

                            _renderers.Clear();
                            _positions.Clear();
                        }
                    }
                }
            }
        }

        public void Reset()
        {
            // _renderers.ToList().ForEach(n => { Destroy(n.gameObject); });
            //
            // _renderers.Clear();
            // _positions.Clear();
            // Result = null;
            //
            Destroy(gameObject);
        }
    }
}
