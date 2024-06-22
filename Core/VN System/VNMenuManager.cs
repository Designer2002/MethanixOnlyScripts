using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VISUALNOVEL
{
    public class VNMenuManager : MonoBehaviour
    {
        public static VNMenuManager instance;
        private Page activePage = null;
        private bool isOpen = false;

        [SerializeField]
        private Page[] pages;
        [SerializeField]
        private CanvasGroup root;
        [SerializeField]
        private DIALOGUE.CanvasGroupController rootCG;
        public Camera mainCamera;


        private Page GetPage(Page.PageType pageType)
        {
            return pages.FirstOrDefault(page => pageType == page.pageType);
        }

        private void Awake()
        {
            instance = this;
            VariableStore.CreateVariable<string>(name: "backgroundPanel", defaultValue: "");
        }

        private void Start()
        {
            rootCG = new DIALOGUE.CanvasGroupController(this, root);
        }

        public void OpenSavePage()
        {
            var page = GetPage(Page.PageType.SAVE_AND_LOAD);
            var slm = page.anim.GetComponentInParent<SaveAndLoad>();
            slm.menuFunction = SaveAndLoad.MenuFunction.SAVE;
            DealWithPage(page);
        }

        public void OpenLoadPage()
        {
            var page = GetPage(Page.PageType.SAVE_AND_LOAD);
            var slm = page.anim.GetComponentInParent<SaveAndLoad>();
            slm.menuFunction = SaveAndLoad.MenuFunction.LOAD;
            DealWithPage(page);
        }

        public void OpenConfigPage()
        {
            var page = GetPage(Page.PageType.CONFIG);
            DealWithPage(page);
        }

        public void OpenRulesPage()
        {
            var page = GetPage(Page.PageType.RULES);
            DealWithPage(page);
        }

        private void DealWithPage(Page page)
        {
            if (page == null) return;
            if (activePage != null && activePage != page)
            {
                activePage.Close();
            }
            page.Open();
            activePage = page;

            if (!isOpen) OpenRoot();
        }

        public void OpenRoot()
        {
            rootCG.Show();
            if(DIALOGUE.DialogueSystem.instance != null) DIALOGUE.DialogueSystem.instance.DialogueContainer.hide();
            rootCG.SetInteractableState(true);
            isOpen = true;
        }

        public void CloseRoot()
        {
            rootCG.Hide();
            if (DIALOGUE.DialogueSystem.instance != null) DIALOGUE.DialogueSystem.instance.DialogueContainer.show();
            rootCG.SetInteractableState(false);
            isOpen = false;
        }

        public void ClickHome()
        {
            VN_ConfigurationsData.activeConfig.Save();
            UnityEngine.SceneManagement.SceneManager.LoadScene(MainMenu.MAIN_MENU_SCENE_NAME);
        }
        
        public void ClickQuit()
        {
            Application.Quit();
        }
    }
}