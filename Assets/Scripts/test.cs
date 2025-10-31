using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using X2;

namespace X
{
    public class test : MonoBehaviour
    {
        public test2 Test2;
        public GameObject Test22;
        public GameObject prefab;
        public static test Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
            if(Instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Escape update");
            }
            EscapeFunction();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                test2.TestMethod();
            }
            if(Input.GetKeyDown(KeyCode.A))
            {
                Test2.AnotherMethod();
                Test22.GetComponent<test2>().AnotherMethod();

            }
            if(Input.GetKeyDown(KeyCode.B))
            {
                Test2.AnotherMethod(Test2);
                Test22.GetComponent<test2>().AnotherMethod(Test22.GetComponent<test2>());
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                SceneManager.LoadScene("TestScene2");
            }
            if(Input.GetKeyDown(KeyCode.D))
            {
                Test2.GetA();
                Test22.GetComponent<test2>().GetA();
            }
            if(Input.GetKeyDown(KeyCode.E))
            {
                Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
            }
        }

        private void FixedUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Escape fixed update");
            }
        }

        public void EscapeFunction()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Escape function");
            }
        }
    }
}

