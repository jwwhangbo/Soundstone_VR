//using NUnit.Framework.Constraints;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace SoundStone
{
    public class PlayerToolkit : MonoBehaviour
    {
        private static PlayerToolkit _instance;

        public static PlayerToolkit instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<PlayerToolkit>();
                return _instance;
            }
        }

        private static SoundStone_ToolTypes _selectedTool = SoundStone_ToolTypes.Brush;

        public static SoundStone_ToolTypes selectedTool
        {
            get { return _selectedTool; }
            set { _selectedTool = value; }
        }
    }
}