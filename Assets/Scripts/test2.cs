using UnityEngine;

namespace X2
{
    public class test2:MonoBehaviour
    {
        private int a=5;
        private void Start()
        {
            a = 5;
        }
        public static void TestMethod()
        {
            Debug.Log("This is a test2");
        }

        public void AnotherMethod(test2 test)
        {
            Debug.Log("Another method" + test.gameObject.name);
        }
        public void AnotherMethod()
        {
            Debug.Log("Another method");
        }
        public void GetA()
        {
            Debug.Log("Value of a: " + a);
        }
    }
}
