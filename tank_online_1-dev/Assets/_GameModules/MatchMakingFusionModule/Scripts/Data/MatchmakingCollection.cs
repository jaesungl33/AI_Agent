using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(MatchmakingCollection), menuName = "ScriptableObjects/" + nameof(MatchmakingCollection), order = 1)]
public class MatchmakingCollection : CollectionBase<MatchmakingDocument>
{
    public MatchmakingDocument GetActiveDocument()
    {
        // Find the first document that is selected
        return FindDocumentByProperty(doc => doc.IsSelected, true).DeepClone();
    }

    public void GetDocumentByProperty(Func<MatchmakingDocument, bool> predicate, bool value, Action<MatchmakingDocument> callback)
    {
        MatchmakingDocument foundDocument = FindDocumentByProperty(predicate, value);
        if (foundDocument != null)
        {
            callback?.Invoke(foundDocument.DeepClone());
        }
        else
        {
            Debug.LogWarning("No document found matching the given property.");
            callback?.Invoke(null);
        }
    }

    internal int GetIndex(string matchId)
    {
        int index = -1;
        for (int i = 0; i < documents.Count; i++)
        {
            if (documents[i].matchID == matchId)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public void SetMatchIndex(int index)
    {
        if (index < 0 || index >= documents.Count)
        {
            Debug.LogWarningFormat("Index out of range: " + index);
            return;
        }
        for (int i = 0; i < documents.Count; i++)
        {
            documents[i].IsSelected = (i == index);
        }
    }

    public int GetSelectedMatchIndex()
    {
        for (int i = 0; i < documents.Count; i++)
        {
            if (documents[i].IsSelected)
            {
                return i;
            }
        }
        Debug.LogWarning("No selected document found to get index.");
        return -1; // Return -1 if no document is selected
    }
}