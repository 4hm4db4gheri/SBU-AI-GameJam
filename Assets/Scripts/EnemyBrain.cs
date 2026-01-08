using UnityEngine;
using System.Collections.Generic;
using System.Linq; // برای استفاده از توابع لیست

public class EnemyBrain : MonoBehaviour
{
    public enum State { NearPlayer, NearAlly, LowHealth, Confused }
    public enum Action { AttackPlayer, AttackAlly, Flee }

    private static Dictionary<string, float> qTable = new Dictionary<string, float>();

    private float learningRate = 0.5f;
    private float discountFactor = 0.9f;
    private float epsilon = 0.3f;

    public Action ChooseAction(State currentState)
    {
        // 1. اکتشاف (Exploration): انتخاب شانسی از بین تمام گزینه‌ها
        if (Random.value < epsilon)
        {
            // این خط به طور خودکار هر تعداد اکشن داشته باشی (حمله، فرار و...) یکی را شانسی انتخاب می‌کند
            System.Array values = System.Enum.GetValues(typeof(Action));
            return (Action)values.GetValue(Random.Range(0, values.Length));
        }

        // 2. بهره‌برداری (Exploitation): پیدا کردن بهترین امتیاز بین همه گزینه‌ها
        float bestValue = -9999f;
        Action bestAction = Action.AttackPlayer; // پیش‌فرض

        // حلقه روی تمام اکشن‌های ممکن (حمله به پلیر، حمله به یار، فرار)
        foreach (Action action in System.Enum.GetValues(typeof(Action)))
        {
            float val = GetQValue(currentState, action);
            if (val > bestValue)
            {
                bestValue = val;
                bestAction = action;
            }
        }

        return bestAction;
    }

    public void Learn(State state, Action action, float reward)
    {
        float currentQ = GetQValue(state, action);

        // یافتن بهترین پاداش ممکن در مرحله بعد (برای فرمول کامل Q-Learning)
        // البته چون ما NextState دقیق نداریم، همین فرمول ساده شده شما هم خوب کار می‌کند
        // اما برای دقیق‌تر شدن، فرمول استاندارد این است:
        // Q_new = Q_old + Alpha * (Reward + Gamma * Max_Q_Next - Q_old)

        // فعلاً با همان فرمول ساده شما پیش می‌رویم چون کار را راه می‌اندازد:
        float newQ = currentQ + learningRate * (reward - currentQ);

        SetQValue(state, action, newQ);

        // لاگ فقط وقتی تغییر مهمی رخ داد نمایش داده شود تا کنسول شلوغ نشود
        // Debug.Log($"Learned: State {state} + Action {action} -> Reward {reward}");
    }

    private float GetQValue(State s, Action a)
    {
        string key = s.ToString() + "_" + a.ToString();
        if (!qTable.ContainsKey(key)) return 0f;
        return qTable[key];
    }

    private void SetQValue(State s, Action a, float value)
    {
        string key = s.ToString() + "_" + a.ToString();
        if (qTable.ContainsKey(key)) qTable[key] = value;
        else qTable.Add(key, value);
    }

    public static void ResetBrain()
    {
        qTable.Clear();
        Debug.Log("Brain Reset!");
    }
}