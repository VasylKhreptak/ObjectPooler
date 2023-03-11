using System.Collections;
using Plugins.ObjectPooler.Extensions;
using UnityEngine;

namespace Plugins.ObjectPooler.Linker
{
    public class PositionLinker : MonoBehaviour
    {
        private LinkerData _data;

        private Coroutine _linkCoroutine;

        public void StartUpdating(LinkerData data)
        {
            if (_linkCoroutine != null) return;

            _data = data;

            if (IsDataValid() == false)
            {
                return;
            }

            _linkCoroutine = StartCoroutine(LinkRoutine());
        }

        public void StopUpdating()
        {
            if (_linkCoroutine != null)
            {
                StopCoroutine(_linkCoroutine);

                _linkCoroutine = null;

                _data = null;
            }
        }

        #region MonoBehaviour

        private void OnDisable()
        {
            StopUpdating();
        }

        #endregion

        #region DataValidation

        private bool IsDataValid()
        {
            if (_data == null)
            {
                Debug.LogWarning("Linker data is null.Cannot start linking!");

                return false;
            }

            if (_data.linkTo == null)
            {
                Debug.LogWarning("Target is null.Cannot start linking!");

                return false;
            }

            if (_data.axes.Is01() == false)
            {
                Debug.LogWarning("Axis is not valid.Cannot start linking!");

                return false;
            }

            return true;
        }

        #endregion

        private IEnumerator LinkRoutine()
        {
            while (true)
            {
                if (CanContinueLink())
                {
                    Link();
                }
                else
                {
                    StopUpdating();
                }

                yield return null;
            }
        }

        private bool CanContinueLink()
        {
            if (_data.linkTo == null)
            {
                Debug.LogWarning("Target is null.Cannot continue linking!Stopped linking!");
                return false;
            }

            return true;
        }

        private void Link()
        {
            transform.position = transform.position.ReplaceWithAxes(_data.axes, _data.linkTo.position + _data.offset);
        }
    }
}
