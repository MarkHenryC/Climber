﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using QuiteSensible;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class NewTestScript
    {
        // A Test behaves as an ordinary method
        [Test]
        public void NewTestScriptSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator NewTestScriptWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.

            GameObject landscape = MonoBehaviour.Instantiate(Resources.Load<GameObject>("LandscapePrefab"));
            LevelCreator lc = landscape.GetComponent<LevelCreator>();
            lc.CreateLandscapeMesh();

            yield return null;
        }
    }
}