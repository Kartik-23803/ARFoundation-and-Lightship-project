using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "QuestionSet", menuName = "AR Game/Question Set")]
public class QuestionSetSO : ScriptableObject
{
    [System.Serializable]
    public class QuestionData
    {
        public Sprite questionImage;
        public bool correctAnswer;

        [Header("Supporting Image")]
        public bool hasSupportingImage;
        public Sprite supportingImage;
    }

    public List<QuestionData> questions = new List<QuestionData>();
}