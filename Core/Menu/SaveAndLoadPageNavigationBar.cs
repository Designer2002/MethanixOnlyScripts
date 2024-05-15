using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VISUALNOVEL
{
    public class SaveAndLoadPageNavigationBar : MonoBehaviour
    {
        [SerializeField] private SaveAndLoad menu;
        private bool initialized = false;
        [SerializeField] private GameObject ButtonLine;
        [SerializeField] private GameObject previousButton;
        [SerializeField] private GameObject nextButton;

        private const int MAX_BUTTONS = 3;

        public int selectedPage = 1;

        private int maxPages = 0;

        private List<Button> btns = new List<Button>();

        private void InitializeMenu()
        {
            if(initialized)
                return;
            initialized = true;

            maxPages = Mathf.CeilToInt((float)SaveAndLoad.MAX_FILES / menu.slotsPerPage);
            int pageButtonLimit = MAX_BUTTONS < maxPages ? MAX_BUTTONS : maxPages;
            var b = ButtonLine.GetComponentsInChildren<Button>();

            for (int i = 1; i < b.Length - 1; i++)
            {
                btns.Add(b[i]);
            }
            Paint();
            previousButton.GetComponent<Button>().interactable = false;
            //for (int i = 0; i < pageButtonLimit; i++)
            //{
            //    GameObject ob = Instantiate(buttonPrefab.gameObject, buttonPrefab.transform.parent);
            //    ob.SetActive(true);
            //    Button button = ob.GetComponent<Button>();
            //    ob.name = i.ToString();
            //    //TMPro.TextMeshProUGUI txt = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            //    //txt.text = i.ToString();
            //    int closureIndex = i;
            //    button.image.sprite = Resources.Load<Sprite>($"{FilePaths.resources_menu_gui_path}circle1");
            //    button.onClick.AddListener(() => SelectSaveFilePage(closureIndex));
            //    btns.Add(button);
            //}
            
            //previousButton.gameObject.SetActive(selectedPage > 1);
            //nextButton.gameObject.SetActive(selectedPage < maxPages);
        }

        private void SelectSaveFilePage(int pageNumber)
        {
            selectedPage = pageNumber;
            menu.PopulateSaveSlotsForPage(pageNumber);
            if (pageNumber == maxPages) nextButton.GetComponent<Button>().interactable = false;
            else nextButton.GetComponent<Button>().interactable = true;
            if (pageNumber == 1) previousButton.GetComponent<Button>().interactable = false;
            else previousButton.GetComponent<Button>().interactable = true;
        }

        private void Paint()
        {
            foreach(var b in btns)
            {
                b.image.sprite = Resources.Load<Sprite>($"{FilePaths.resources_menu_gui_path}circle1");
            }
            //Debug.Log(selectedPage - 1);
            btns[selectedPage-1].image.sprite = Resources.Load<Sprite>($"{FilePaths.resources_menu_gui_path}circle2");
        }
        private void Start()
        {
            InitializeMenu();
            
        }

        public void ToNextPage()
        {

            SelectSaveFilePage(selectedPage + 1);
            Paint();
            previousButton.GetComponent<Button>().interactable = true;

        }

        public void ToPreviousPage()
        {
            
            SelectSaveFilePage(selectedPage - 1);
            Paint();
            nextButton.GetComponent<Button>().interactable = true;
            
        }
    }
}