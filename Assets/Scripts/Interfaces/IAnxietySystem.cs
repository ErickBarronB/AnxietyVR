public interface IAnxietySystem
{
    void AddAnxiety(float amount);
    void RemoveAnxiety(float amount);
    float GetAnxiety();
    bool GetIsAnxious();
    Enum_AnxietyLevel GetAnxietyLevel();
    bool GetIsIncreasing();
    void SetIsIncreasing(bool NewIncreasing);
    void AddAnxietyTrigger(int TriggerAmounts);
    void RemoveAnxietyTrigger(int TriggerAmounts);
}