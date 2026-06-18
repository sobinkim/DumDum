using UnityEngine;

namespace SB.App.Application
{
    [CreateAssetMenu(menuName = "DumDum/Worry Memo Design Manager", fileName = "WorryMemoDesignManager")]
    public sealed class WorryMemoDesignManager : ScriptableObject
    {
        [SerializeField] private Sprite[] memoSprites;

        public Sprite GetMemoSprite(int designIndex)
        {
            if (memoSprites == null || memoSprites.Length <= 0)
                return null;

            int index = (designIndex & int.MaxValue) % memoSprites.Length;
            return memoSprites[index];
        }

        public void SetMemoSprites(Sprite[] sprites)
        {
            memoSprites = sprites;
        }
    }
}
