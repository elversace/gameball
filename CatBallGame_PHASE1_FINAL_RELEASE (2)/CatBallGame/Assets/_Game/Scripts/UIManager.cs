using UnityEngine;
using UnityEngine.UI;
using Game.Core;
namespace Game.UI
{
    public class UIManager:MonoBehaviour
    {
        [SerializeField] private GameObject winPanel,failPanel; [SerializeField] private Button retryButton,retryButtonFail;
        private void OnEnable(){GameEvents.OnLevelWin+=ShowWin;GameEvents.OnLevelFail+=ShowFail;}
        private void OnDisable(){GameEvents.OnLevelWin-=ShowWin;GameEvents.OnLevelFail-=ShowFail;retryButton?.onClick.RemoveListener(Retry);retryButtonFail?.onClick.RemoveListener(Retry);}
        private void Start(){retryButton?.onClick.AddListener(Retry);retryButtonFail?.onClick.AddListener(Retry);}
        private void ShowWin(){if(winPanel)winPanel.SetActive(true);if(failPanel)failPanel.SetActive(false);}
        private void ShowFail(){if(failPanel)failPanel.SetActive(true);if(winPanel)winPanel.SetActive(false);}
        private void HideAll(){if(winPanel)winPanel.SetActive(false);if(failPanel)failPanel.SetActive(false);}
        public void Configure(GameObject win, GameObject fail, Button winRetry, Button failRetry){winPanel=win;failPanel=fail;retryButton=winRetry;retryButtonFail=failRetry;HideAll();}
        private void Retry(){GameManager.Instance?.Retry();HideAll();}
    }
}
