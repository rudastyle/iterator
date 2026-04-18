using UnityEngine;

namespace TimeLoop
{
    [CreateAssetMenu(menuName = "TimeLoop/StageDatabase", fileName = "StageDatabase")]
    public class StageDatabase : ScriptableObject
    {
        public StageData[] stages;
    }
}
