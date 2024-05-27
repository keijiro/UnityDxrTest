using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEditor;
using UnityEditorInternal;

[InitializeOnLoad]
public sealed class PathTracingAwaker
{
    static PathTracingAwaker()
      => EditorApplication.update += Update;

    static void Update()
    {
        // Play mode
        if (Application.isPlaying) return;

        // HDRP
        var hdrp = RenderPipelineManager.currentPipeline as HDRenderPipeline;
        if (hdrp == null) return;

        // Volume manager
        if (!VolumeManager.instance.isInitialized) return;

        // Path tracing in volume profile
        var hdcam = HDCamera.GetOrCreate(Camera.main);
        var profile = hdcam.volumeStack.GetComponent<PathTracing>();
        if (!profile.enable.value) return;

        // Convergence
        if (hdrp.IsFrameCompleted(hdcam)) return;

        // Game view repaint
        InternalEditorUtility.RepaintAllViews();
    }
}
