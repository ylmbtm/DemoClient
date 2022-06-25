using UnityEngine;
using System.Collections;
using Protocol;

public class MLRaid : GTSingleton<MLRaid>
{
    public ERewardState GetChapterRewardState(int chapter, int index)
    {
        if (!GTChapterData.HasItem(chapter))
        {
            return ERewardState.NOT_RECEIVE;
        }
        ChapterItem vo = GTChapterData.GetItem(chapter);
        DCopyMainChapter db = ReadCfgCopyMainChapter.GetDataById(chapter);
        int starNum = GetChapterStarNumByChapter(chapter);
        if (starNum < db.Stars[index])
        {
            return ERewardState.NOT_RECEIVE;
        }
        int s = db.Awards[index];
        return s == 1 ? ERewardState.HAS_RECEIVE : ERewardState.CAN_RECEIVE;
    }

    public int GetMaxCanEnterCopyIndex(int difficulty, int chapter)
    {
        return 1;
    }

    public int GetCopyStarNumById(int id)
    {
        if(GTCopyData.HasItem(id))
        {
            return GTCopyData.GetItem(id).StarNum;
        }

        return 0;
    }

    public int GetChapterStarNumByChapter(int chapter)
    {
        int starNum = 0;
        if (GTChapterData.HasItem(chapter))
        {
            DCopyMainChapter db = ReadCfgCopyMainChapter.GetDataById(chapter);
            for (int i = 0; i < db.Copys.Length; i++)
            {
                starNum += GetCopyStarNumById(db.Copys[i]);
            }
        }
        return starNum;
    }
}
