using System;

using UnityEngine;

namespace EWova.Wristband
{
    [RequireComponent(typeof(Wristband))]
    public class Setup : MonoBehaviour
    {
        public string Flag = "GO_TO_EWOVA,CAPTURE_TO_EWOVA,SHARE_TO_EWOVA,VIEW_LEARNING_PROFILE,EXPLORE_EWOVA_WEBSITE,QUIT_APP";

        private void Start()
        {
            GetComponent<Wristband>().LoadFlag(Flag);
        }
    }
}
