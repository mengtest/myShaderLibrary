/********************************************************************************
** auth:  https://github.com/HushengStudent
** date:  2019/06/25 00:04:02
** desc:  CommandBuffer Mgr;
*********************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Framework
{
    public class CommandBufferMgr : MonoSingleton<CommandBufferMgr>
    {
        private CommandBuffer _commandBuffer;
        private Camera _camera;
        private List<Renderer> _rendererList;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _commandBuffer = new CommandBuffer
            {
                name = "CommandBufferMgr"
            };
            var cameraObject = new GameObject("CommandBufferMgr_Camera");
            cameraObject.SetActive(true);
            _camera = cameraObject.AddComponent<Camera>();
            //_camera.cullingMask ������;
            _camera.clearFlags = CameraClearFlags.Depth;
            _camera.orthographic = true;
            _camera.allowDynamicResolution = false;
            _camera.useOcclusionCulling = false;
            _camera.allowHDR = false;
            _camera.allowMSAA = false;
            _camera.enabled = false;
            _rendererList = new List<Renderer>();
        }

        protected override void OnUninitialize()
        {
            base.OnUninitialize();
            if (_commandBuffer != null)
            {
                _commandBuffer.Release();
                _commandBuffer = null;
            }
            if (_camera)
            {
                Destroy(_camera.gameObject);
                _camera = null;
            }
            _rendererList.Clear();
            _rendererList = null;
        }

        protected override void UpdateEx(float interval)
        {
            base.UpdateEx(interval);
            //��ε��õ�����ִ��;
        }

        /// <summary>
        /// 2D�����Ⱦ;
        /// </summary>
        /// <param name="target">Ŀ��</param>
        /// <param name="width">RT��</param>
        /// <param name="high">RT��</param>
        /// <param name="camWidth">�����</param>
        /// <param name="camHigh">�����</param>
        /// <param name="originalCamSize">ԭʼ�����Size</param>
        /// <param name="camScale">�����Scale</param>
        /// <param name="mat">����</param>
        /// <returns></returns>
        public CommandBufferRender DrawRenderer(GameObject target, int width, int high, int camWidth, int camHigh
            , float originalCamSize, Vector3 camScale, Material mat = null)
        {
            if (!target || !_camera)
            {
                return null;
            }
            _rendererList.Clear();
            _rendererList.Add(target.GetComponent<Renderer>());
            _rendererList.AddRange(target.GetComponentsInChildren<Renderer>(true));
            if (_rendererList.Count < 1)
            {
                return null;
            }

            _rendererList.Sort((r1, r2) => { return r1.rendererPriority > r2.rendererPriority ? 0 : 1; });

            var x = width / camWidth / camScale.x;
            var y = high / camHigh / camScale.y;

            _camera.orthographicSize = originalCamSize * y;
            _camera.rect = new Rect(0, 0, x, y);

            var render = PoolMgr.singleton.GetCsharpObject<CommandBufferRender>(width, high);
            _commandBuffer.Clear();
            _commandBuffer.SetRenderTarget(render.RenderTexture);
            _commandBuffer.ClearRenderTarget(true, true, Color.clear);
            _commandBuffer.SetViewProjectionMatrices(_camera.worldToCameraMatrix, _camera.projectionMatrix);

            foreach (var r in _rendererList)
            {
                var targetMat = mat ?? (r.sharedMaterial ?? r.material);
                if (targetMat && r && r.gameObject.activeSelf)
                {
                    _commandBuffer.DrawRenderer(r, targetMat);
                }
            }
            _rendererList.Clear();

            Graphics.ExecuteCommandBuffer(_commandBuffer);

            //Ȼ���������Ĳ���ʹ������RT��Ϊ������
            //this.GetComponent<Renderer>().sharedMaterial.mainTexture = renderTexture;

            //Camera.main.AddCommandBuffer(CameraEvent.AfterForwardOpaque, CommandBuffer);
            //Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);

            return render;
        }
    }
}