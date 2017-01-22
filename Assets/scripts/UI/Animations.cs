using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class CoroutineAnimations {

    public static IEnumerator Interpolate(float duration, Action<float> onProgress, bool fixedUpdate = false) {
        for (float p = 0; p < 1; p += (fixedUpdate? Time.fixedDeltaTime : Time.deltaTime) / duration ) {
            onProgress(p);
            yield return fixedUpdate? new WaitForFixedUpdate() : null;
        }
        onProgress(1);
    }

    public static Task Interpolate(this MonoBehaviour b, float duration, Action<float> onProgress, bool fixedUpdate = false, CancellationToken cancellation = default(CancellationToken)) {
        var cr = InterpolateCR(duration, onProgress, fixedUpdate, cancellation);
        if (!fixedUpdate)
            return b.RunTask(cr);
        return Task.Factory.StartNew(cr);
    }

    static IEnumerator InterpolateCR(float duration, Action<float> onProgress, bool fixedUpdate = false, CancellationToken cancellation = default(CancellationToken)) {
        for (float p = 0; p < 1 && !cancellation.IsCancellationRequested; p += (fixedUpdate? Time.fixedDeltaTime : Time.deltaTime) / duration ) {
            onProgress(p);
            yield return fixedUpdate? new WaitForFixedUpdate() : null;
        }
        if (!cancellation.IsCancellationRequested) {
            onProgress(1);
        }
    }
}