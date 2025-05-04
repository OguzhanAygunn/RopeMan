using System;

// Remove this line and the associated #endif to enable the custom logging class
#if REMOVE_THIS_IF_CHECK_TO_ENABLE

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DinoFracture
{
    /// <summary>
    /// Example custom DinoFracture logging handler.
    /// 
    /// The logger will automatically be hooked by Unity
    /// via the *InitializeOnLoadMethod attributes.
    /// </summary>
    public class CustomLogger
    {
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        static void InitializeLogger()
        {
            // This actually hooks up the logger. It can be done by a regular
            // MonoBehaviour if desired.
            DinoFracture.Logger.UserLogHandler = OnDinoFractureMessage;
        }

        private static void OnDinoFractureMessage(DinoFracture.LogLevel level, string message, UnityEngine.Object context)
        {
            message = "Custom DinoFracture Logger: " + message;

            switch (level)
            {
                case DinoFracture.LogLevel.Debug:
                    UnityEngine.Debug.Log(message, context);
                    break;

                case DinoFracture.LogLevel.Info:
                case DinoFracture.LogLevel.UserDisplayedInfo:
                    UnityEngine.Debug.Log(message, context);
                    break;

                case DinoFracture.LogLevel.Warning:
                case DinoFracture.LogLevel.UserDisplayedWarning:
                    UnityEngine.Debug.LogWarning(message, context);
                    break;

                case DinoFracture.LogLevel.Error:
                case DinoFracture.LogLevel.UserDisplayedError:
                    UnityEngine.Debug.LogError(message, context);
                    break;

                case LogLevel.Statistic:
                    UnityEngine.Debug.Log(message, context);
                    break;
            }
        }
    }
}

#endif // REMOVE_THIS_IF_CHECK_TO_ENABLE