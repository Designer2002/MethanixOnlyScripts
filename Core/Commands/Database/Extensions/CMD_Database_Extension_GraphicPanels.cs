using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GRAPHICS;
using System;
using System.Linq;

namespace COMMANDS
{
    public class CMD_Database_Extension_GraphicPanels : CMD_DatabseExtensions
    {
        private static string[] PARAM_PANEL = new[] { "-p", "-panel" };
        private static string[] PARAM_LAYER = new[] { "-l", "-layer" };
        private static string[] PARAM_GRAPHIC = new[] { "-g", "-graphic" };
        private static string[] PARAM_SPEED = new[] { "-spd", "-speed" };
        private static string[] PARAM_IMMEDIATE = new[] { "-i", "-immediate" };
        private static string[] PARAM_BLENDTEX = new[] { "-b", "-blend" };
        private static string[] PARAM_AUDIO = new[] { "-au", "-audio" };
        private static string[] PARAM_OFFSET = new[] { "-o", "-offset" };

        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("img", new Func<string[], IEnumerator>(SetLayerMedia));
            database.AddCommand("clear", new Func<string[], IEnumerator>(ClearLayerMedia));
        }

        private static IEnumerator ClearLayerMedia(string[] data)
        {
            var dataList = data.ToList();
            dataList.Insert(0, "-p");
            dataList.Insert(2, "-g");
            data = dataList.ToArray();

            string panelName = "";
            int layer = 0;
            float transitionSpeed = 1f;
            bool immediate = false;
            string blendTextName = "";
            Texture blend = null;

            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_PANEL, out panelName);
            GraphicPanel panel = GraphicPanelManager.instance.GetPanel(panelName);
            if (panel == null)
            {
                Debug.Log($"panel {panelName} doesn't exist!!");
                yield break;
            }
            parameters.TryGetValue(PARAM_LAYER, out layer, defaultValue: -1);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            if (!immediate)
                parameters.TryGetValue(PARAM_SPEED, out transitionSpeed, defaultValue: 1);
            parameters.TryGetValue(PARAM_BLENDTEX, out blendTextName);
            if (!immediate && blendTextName != string.Empty && !string.IsNullOrWhiteSpace(blendTextName))
            {
                blend = Resources.Load<Texture>(FilePaths.GetPath(FilePaths.resources_graphics_transition, blendTextName));
            }
            if (layer == -1) panel.Clear(transitionSpeed, blend, immediate);
            else
            {
                GraphicLayer glayer = panel.GetLayer(layer);
                if (glayer == null) yield break;
                glayer.Clear(transitionSpeed, blend, immediate);
            }
        }

        private static IEnumerator SetLayerMedia(string[] data)
        {
            var dataList = data.ToList();
            dataList.Insert(0, "-p");
            dataList.Insert(2, "-g");
            data = dataList.ToArray();

            string panelName = "";
            int layer = 0;
            string mediaName = "";
            float transitionSpeed = 1f;
            bool immediate = false;
            string blendTextName = "";
            bool useAudio = false;
            bool offset = false;

            string pathToGaphic = "";
            UnityEngine.Object graphic = null;
            Texture blend = null;

            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_PANEL, out panelName);
            GraphicPanel panel = GraphicPanelManager.instance.GetPanel(panelName);
            if (panel == null)
            {
                Debug.Log($"panel {panelName} doesn't exist!!");
                yield break;
            }
            parameters.TryGetValue(PARAM_LAYER, out layer, defaultValue: 0);
            parameters.TryGetValue(PARAM_GRAPHIC, out mediaName);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            parameters.TryGetValue(PARAM_OFFSET, out offset, defaultValue: false);
            if (!immediate)
                parameters.TryGetValue(PARAM_SPEED, out transitionSpeed, defaultValue: 1);
            parameters.TryGetValue(PARAM_BLENDTEX, out blendTextName);
            parameters.TryGetValue(PARAM_AUDIO, out useAudio, defaultValue: false);

            pathToGaphic = FilePaths.GetPath(FilePaths.resources_graphics_bg_photo, mediaName);
            graphic = Resources.Load<Texture>(pathToGaphic);
            if(graphic == null)
            {
                pathToGaphic = FilePaths.GetPath(FilePaths.resources_graphics_bg_video, mediaName);
                graphic = Resources.Load<UnityEngine.Video.VideoClip>(pathToGaphic);
            }
            if(graphic == null)
            {
                Debug.LogWarning("NULL GRAPHIC");
                yield break;
            }
            if(!immediate && blendTextName != string.Empty && !string.IsNullOrWhiteSpace(blendTextName))
            {
                blend = Resources.Load<Texture>(FilePaths.GetPath(FilePaths.resources_graphics_transition, blendTextName));
            }
            GraphicLayer gl = panel.GetLayer(layer, true);
            if (graphic is Texture) yield return gl.SetTexture(graphic as Texture, path: pathToGaphic,transitionSpeed: transitionSpeed, blend, immediate: immediate, offset: offset);
            if (graphic is UnityEngine.Video.VideoClip) yield return gl.SetVideo(graphic as UnityEngine.Video.VideoClip, path: pathToGaphic, transitionSpeed: transitionSpeed, blend, useAudio: useAudio, immediate: immediate, offset: offset);
        }

    }
}