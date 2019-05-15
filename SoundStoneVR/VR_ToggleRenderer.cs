using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoundStone;

namespace SoundStone
{
    [RequireComponent(typeof(MeshRenderer))]
    public class VR_ToggleRenderer : MonoBehaviour
    {
        private MeshRenderer mRenderer;
        // Start is called before the first frame update
        void Start()
        {
            mRenderer = GetComponent<MeshRenderer>();
        }

        public void ToggleRenderer()
        {
            if (mRenderer)
            {
                mRenderer.enabled = !mRenderer.enabled;
            }
        }
    }
}
