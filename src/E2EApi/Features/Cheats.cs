using UnityEngine;

namespace E2EApi.Features
{
    /// <summary>
    /// Play-mode cheats for testing custom maps. All of them act on the live
    /// level and are not persisted.
    /// </summary>
    public static class Cheats
    {
        /// <summary>Knock out every guard on the map. Returns the count.</summary>
        public static int KnockOutGuards()
        {
            int count = 0;
            foreach (var guard in Object.FindObjectsOfType<AICharacter_Guard>())
            {
                if (KnockOut(guard))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>Knock out every guard dog on the map. Returns the count.</summary>
        public static int KnockOutDogs()
        {
            int count = 0;
            foreach (var dog in Object.FindObjectsOfType<AICharacter_Dog>())
            {
                if (KnockOut(dog))
                {
                    count++;
                }
            }
            return count;
        }

        private static bool KnockOut(AICharacter ai)
        {
            var character = ai != null ? ai.m_Character : null;
            if (character == null || character.GetIsKnockedOut())
            {
                return false;
            }
            character.SetIsKnockedOut(knockedOut: true, null);
            return true;
        }
    }
}
