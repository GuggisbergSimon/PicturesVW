using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Test", order = 1)]
    public class TestSO : ScriptableObject
    {
        public string name;
        public int test;
        public Vector3[] tests;
    }
}