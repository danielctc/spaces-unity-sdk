using UnityEngine;
using System.Collections;
using System;

public interface IGLBHandler
{
    System.Collections.IEnumerator LoadGLB(string glbPath, Transform parentTransform = null);
    void OnGLBLoaded(GameObject glbInstance);
} 