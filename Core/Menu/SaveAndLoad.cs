using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VISUALNOVEL
{
    public class SaveAndLoad : Page
    {
        public static SaveAndLoad instance { get; private set; }
        public const int MAX_FILES = 18;
        private string savePath => FilePaths.game_saves;
        private int currentPage = 1;
        private bool loadedFilesForFirstTime = false;
        public enum MenuFunction { SAVE, LOAD }

        public int slotsPerPage => SaveSlots.Length;

        public MenuFunction menuFunction = MenuFunction.SAVE;

        public Texture emptyFileImage;

        public SaveLoadSlot[] SaveSlots;
        // Start is called before the first frame update
        public override void Open()
        {
            base.Open();
            if (!loadedFilesForFirstTime)
                PopulateSaveSlotsForPage(currentPage);

        }
        private void Awake()
        {
            instance = this;
        }
        public void PopulateSaveSlotsForPage(int pageNumber)
        {
            currentPage = pageNumber;
            int startingFile = ((currentPage - 1) * slotsPerPage) + 1;
            int endingFile = startingFile + slotsPerPage - 1;
            for (int i = 0; i < slotsPerPage; i++)
            {
                int fileNum = startingFile + i;
                SaveLoadSlot slot = SaveSlots[i];
                if (fileNum <= MAX_FILES)
                {
                    slot.root.SetActive(true);
                    string filePath = $"{FilePaths.game_saves}{fileNum}{VNGameSave.FILE_TYPE}";
                    slot.fileNumber = fileNum;
                    slot.filePath = filePath;
                    slot.PopulateDetails(menuFunction);
                }
                else
                {
                    slot.root.SetActive(false);
                }
            }
        }

    }

}