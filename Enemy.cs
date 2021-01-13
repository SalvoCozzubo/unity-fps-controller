interface Enemy {
    string GetName();
    int GetHealth();
    int GetMaxHealth();
    int GetShotDamage();
    float GetShotRate();
    void OnDeath();
    void OnSpawn();
}
