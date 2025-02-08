using UnityEngine;

namespace Scripting
{
    public class SprayPaintSounds : MonoBehaviour
    {
        [SerializeField]
        GameObject sprayPaintCan;
        [SerializeField] AudioSource spraySound;
        [SerializeField] AudioSource rattleSound;

        void ShowPaintCan(){
            sprayPaintCan.SetActive(true);
        }   
        public void HidePaintCan(){
            sprayPaintCan.SetActive(false);
        }

        public void StartSpraySound()
        {
            spraySound.Play();
        }

        public void StopSpraySound()
        {
            spraySound.Stop();
        }

        public void StartRattleSound()
        {
            rattleSound.Play();
        }

        public void StopRattleSound()
        {
            rattleSound.Stop();
        }
    }
}
