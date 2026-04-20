using System;
using MoreMountains.TopDownEngine;

namespace PixelCrushers.TopDownEngineSupport
{
    /// <summary>
    /// If you want to use the Pixel Crushers save system instead of TDE's,
    /// you can use this script as a starting point. It saves various attributes
    /// on the character's Health component.
    /// </summary>
    public class TDECharacterSaver : Saver
    {
        [Serializable]
        public class Data
        {
            public float initialHealth;
            public float maxHealth;
            public float currentHealth;
            public bool invulnerable;
            public bool immuneToKnockback;
        }

        protected Data m_data = new Data();
        protected Health m_health;

        public override void Awake()
        {
            base.Awake();
            m_health = GetComponent<Health>();
        }

        public override string RecordData()
        {
            if (m_health == null) return string.Empty;
            m_data.initialHealth = m_health.InitialHealth;
            m_data.maxHealth = m_health.MaximumHealth;
            m_data.currentHealth = m_health.CurrentHealth;
            m_data.invulnerable = m_health.Invulnerable;
            m_data.immuneToKnockback = m_health.ImmuneToKnockback;
            return SaveSystem.Serialize(m_data);
        }

        public override void ApplyData(string s)
        {
            if (string.IsNullOrEmpty(s)) return;
            m_data = SaveSystem.Deserialize<Data>(s, m_data);
            if (m_data == null)
            {
                m_data = new Data();
            }
            else
            {
                m_health.InitialHealth = m_data.initialHealth;
                m_health.MaximumHealth = m_data.maxHealth;
                m_health.SetHealth(m_data.currentHealth);
                m_health.Invulnerable = m_data.invulnerable;
                m_health.ImmuneToKnockback = m_data.immuneToKnockback;
            }
        }

    }
}
