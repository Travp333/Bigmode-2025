using System.Collections;
using System.Linq;
using UnityEngine;

namespace Scripting
{
    public class ShiftManager : MonoBehaviour
    {
        [SerializeField] private Light directionalLight;
        [SerializeField] private Renderer skybox;
        [SerializeField] private Light[] insideLights;
        [SerializeField] private Light[] outsideLights;
        [SerializeField] private Light[] spotLights;
        [SerializeField] private AnimationCurve lightIntensity;

        [SerializeField] private Light[] nightSpotLights;

        // [SerializeField] [Range(0, 1)] private float debug;
        [SerializeField] private bool isNightTime;

        [SerializeField] private ClockPointer clockPointer;

        [SerializeField] private MeshRenderer openRenderer;

        private Material _skybox;
        private Material _ceilings;

        private readonly Quaternion _startRotationLight =
            new(0.707079887f, 0.00617067516f, -0.707079887f, 0.00617067516f);

        private readonly Quaternion _endRotationLight = new(0.0061706081f, 0.707079887f, -0.0061706081f, 0.707079887f);

        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int Color1 = Shader.PropertyToID("_Color");

        private Material[] materials;
        private Color[] colors;

        void Awake()
        {
            _ceilings = skybox.materials[0];
            _skybox =
                skybox.materials[6];

            materials = openRenderer.materials;
            colors = materials.Select(n => n.GetColor("_EmissionColor")).ToArray();

            LerpShiftState(0);
            _isNightTime = true;
            StartCoroutine(AnimateToNightTime());
        }

        void Start()
        {
            materials.ToList().ForEach(mat =>
            {
                mat.DisableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
            });
        }

        public void SetEmission(bool isEnabled)
        {
            for (var i = 0; i < materials.Length; i++)
            {
                var mat = materials[i];
                var color = colors[i];
                if (isEnabled)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", color * 1f); // Adjust intensity as needed
                }
                else
                {
                    mat.DisableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", Color.black);
                }
            }
        }

        public void LerpShiftState(float value)
        {
            directionalLight.transform.rotation = Quaternion.Lerp(_startRotationLight, _endRotationLight, value);
            directionalLight.intensity = lightIntensity.Evaluate(value);

            _skybox.SetColor(EmissionColor,
                Color.Lerp(Color.black, Color.white, lightIntensity.Evaluate(value) * lightIntensity.Evaluate(value)));
            _skybox.SetColor(Color1, Color.Lerp(Color.black, Color.white, lightIntensity.Evaluate(value)));

            _ceilings.SetColor(EmissionColor, Color.white * (1f - lightIntensity.Evaluate(value) * 0.5f));

            for (var i = 0; i < insideLights.Length; i++)
            {
                insideLights[i].intensity = 1f - lightIntensity.Evaluate(value);
            }

            for (var i = 0; i < outsideLights.Length; i++)
            {
                outsideLights[i].intensity = 500f - lightIntensity.Evaluate(value) * 500f;
            }

            for (var i = 0; i < spotLights.Length; i++)
            {
                spotLights[i].intensity = 10f - lightIntensity.Evaluate(value) * 10f;
            }

            clockPointer.SetDayTime(value);
        }


        private bool _isNightTime;


        public void SetIsNightTime(bool value)
        {
            if (value && !_isNightTime)
            {
                _isNightTime = true;
                StartCoroutine(AnimateToNightTime());
            }

            if (!value && _isNightTime)
            {
                _isNightTime = false;
                StartCoroutine(AnimateToDayTime());
            }

            SetEmission(!value);
        }

        private IEnumerator AnimateToNightTime()
        {
            _isAnimating = true;
            var elapsed = 0f;
            while (elapsed < 1.0f)
            {
                elapsed += Time.deltaTime;

                for (var i = 0; i < insideLights.Length; i++)
                {
                    insideLights[i].intensity = 1f - elapsed;
                }

                for (var i = 0; i < spotLights.Length; i++)
                {
                    spotLights[i].intensity = 10f - elapsed * 10f;
                }

                for (var i = 0; i < nightSpotLights.Length; i++)
                {
                    nightSpotLights[i].intensity = elapsed * 20f;
                }

                _ceilings.SetColor(EmissionColor, Color.white * (1f - elapsed) * 0.5f);

                yield return null;
            }

            _isAnimating = false;
        }

        private IEnumerator AnimateToDayTime()
        {
            _isAnimating = true;
            var elapsed = 0f;
            while (elapsed < 1.0f)
            {
                elapsed += Time.deltaTime;

                for (var i = 0; i < insideLights.Length; i++)
                {
                    insideLights[i].intensity = elapsed;
                }

                for (var i = 0; i < spotLights.Length; i++)
                {
                    spotLights[i].intensity = elapsed * 10f;
                }

                for (var i = 0; i < nightSpotLights.Length; i++)
                {
                    nightSpotLights[i].intensity = 20f - elapsed * 20f;
                }

                _ceilings.SetColor(EmissionColor, Color.white * elapsed * 0.5f);

                yield return null;
            }

            _isAnimating = false;
        }

        private bool _isAnimating;

        // private void Update()
        // {
        //     if (!_isNightTime && !_isAnimating)
        //         LerpShiftState(debug);
        //
        //     SetIsNightTime(isNightTime);
        // }
    }
}
