using UnityEngine;
using System.Collections;
using TMPro;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Kampf-Präsentation")]
    public float fightScale = 2.2f;
    public float moveSpeed = 4.0f;
    public float fightAnimationSpeed = 0.5f;

    public Vector3 heroOffset = new Vector3(-2.5f, 0, 0);
    public Vector3 enemyOffset = new Vector3(2.5f, 0, 0);

    private Camera mainCam;
    public bool isFighting = false;

    private Vector3 startHeroPos, startEnemyPos;
    private Vector3 startHeroScale, startEnemyScale;

    private int oldHeroBgOrder, oldHeroIconOrder, oldHeroTextOrder;
    private int oldEnemyBgOrder, oldEnemyIconOrder, oldEnemyTextOrder;

    void Awake()
    {
        Instance = this;
        mainCam = Camera.main;
    }

    public void StartCombat(CardController playerCard, CardController enemyCard)
    {
        if (isFighting) return;
        StartCoroutine(CombatRoutine(playerCard, enemyCard));
    }

    IEnumerator CombatRoutine(CardController player, CardController enemy)
    {
        isFighting = true;

        PrepareCardForFight(player, out startHeroPos, out startHeroScale, out oldHeroBgOrder, out oldHeroIconOrder, out oldHeroTextOrder);
        PrepareCardForFight(enemy, out startEnemyPos, out startEnemyScale, out oldEnemyBgOrder, out oldEnemyIconOrder, out oldEnemyTextOrder);

        Vector3 camCenter = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, 0);
        Vector3 heroTargetPos = camCenter + heroOffset;
        Vector3 enemyTargetPos = camCenter + enemyOffset;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;
            player.transform.position = Vector3.Lerp(startHeroPos, heroTargetPos, t);
            enemy.transform.position = Vector3.Lerp(startEnemyPos, enemyTargetPos, t);
            player.transform.localScale = Vector3.Lerp(startHeroScale, startHeroScale * fightScale, t);
            enemy.transform.localScale = Vector3.Lerp(startEnemyScale, startEnemyScale * fightScale, t);
            yield return null;
        }

        EnemyData enemyData = enemy.myData as EnemyData;
        int enemyCurrentHealth = enemyData != null ? enemyData.health : 10;
        int enemyDamage = enemyData != null ? enemyData.damage : 1;
        int enemyDefense = enemyData != null ? enemyData.defense : 0;

        yield return new WaitForSeconds(0.2f);

        while (enemyCurrentHealth > 0 && PlayerManager.Instance.currentHealth > 0)
        {
            // Spieler Schlag
            yield return StartCoroutine(SmashAnimation(player.transform, enemy.transform));

            int dmgToEnemy = Mathf.Max(1, PlayerManager.Instance.attackDamage - enemyDefense);
            enemyCurrentHealth -= dmgToEnemy;
            FloatingTextManager.Instance.Show("-" + dmgToEnemy, enemy.transform.position, Color.red, false);

            yield return StartCoroutine(ShakeAnimation(enemy.transform));

            if (enemyCurrentHealth <= 0) break;
            yield return new WaitForSeconds(0.3f);

            // Gegner Schlag
            yield return StartCoroutine(SmashAnimation(enemy.transform, player.transform));

            PlayerManager.Instance.TakeDamage(enemyDamage);
            FloatingTextManager.Instance.Show("-" + enemyDamage, player.transform.position, Color.red, false);
            yield return StartCoroutine(ShakeAnimation(player.transform));

            if (PlayerManager.Instance.currentHealth <= 0) break;
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(0.3f);
        bool playerWon = PlayerManager.Instance.currentHealth > 0;

        if (playerWon)
        {
            // --- HIER WIRD LOOT GECHECKT ---
            if (enemyData != null) CheckLoot(enemyData);

            yield return StartCoroutine(MoveCardBack(player, startEnemyPos, startHeroScale));
            ResetCardLayer(player, oldHeroBgOrder, oldHeroIconOrder, oldHeroTextOrder);

            enemy.transform.position = startEnemyPos;
            ResetCardLayer(enemy, oldEnemyBgOrder, oldEnemyIconOrder, oldEnemyTextOrder);
            GridManager.Instance.FinishMovement(enemy);
        }
        else
        {
            yield return StartCoroutine(MoveCardBack(player, startHeroPos, startHeroScale));
            ResetCardLayer(player, oldHeroBgOrder, oldHeroIconOrder, oldHeroTextOrder);
            ResetCardLayer(enemy, oldEnemyBgOrder, oldEnemyIconOrder, oldEnemyTextOrder);
            Debug.Log("Player defeated!");
        }

        isFighting = false;
    }

    IEnumerator MoveCardBack(CardController card, Vector3 targetPos, Vector3 targetScale)
    {
        Vector3 startPos = card.transform.position;
        Vector3 startScale = card.transform.localScale;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;
            card.transform.position = Vector3.Lerp(startPos, targetPos, t);
            card.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
    }

    IEnumerator SmashAnimation(Transform attacker, Transform victim)
    {
        Vector3 startPos = attacker.position;
        Vector3 targetPos = victim.position;
        Vector3 impactPoint = Vector3.Lerp(startPos, targetPos, 0.6f);
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / (fightAnimationSpeed / 2);
            attacker.position = Vector3.Lerp(startPos, impactPoint, t);
            yield return null;
        }
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / (fightAnimationSpeed / 2);
            attacker.position = Vector3.Lerp(impactPoint, startPos, t);
            yield return null;
        }
        attacker.position = startPos;
    }

    IEnumerator ShakeAnimation(Transform target)
    {
        Vector3 originalPos = target.position;
        float shakeStrength = 0.15f;
        for (int i = 0; i < 5; i++)
        {
            target.position = originalPos + (Vector3)(Random.insideUnitCircle * shakeStrength);
            yield return new WaitForSeconds(0.05f);
        }
        target.position = originalPos;
    }

    void CheckLoot(EnemyData enemyData)
    {
        // 1. Gold
        float baseGold = Random.Range(enemyData.minGoldDrop, enemyData.maxGoldDrop + 1);
        float multiplier = GridManager.Instance.currentLevel.goldMultiplier;
        int finalAmount = Mathf.RoundToInt(baseGold * multiplier);

        if (finalAmount > 0)
        {
            PlayerManager.Instance.AddMoney(finalAmount);
            string formattedReward = PlayerManager.FormatMoney(finalAmount);
            // Gold bleibt erstmal als Text, da Münz-Icons komplizierter sind
            FloatingTextManager.Instance.Show("+" + formattedReward, Vector3.zero, Color.yellow, true);
        }

        // 2. Items
        if (enemyData.potentialDrops == null) return;

        foreach (var drop in enemyData.potentialDrops)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= drop.dropChance)
            {
                PlayerManager.Instance.AddItemToInventory(drop.item);

                // --- ITEM LOGIK (NEU) ---
                // Zeige das Artwork des gedroppten Items
                if (drop.item.artwork != null)
                {
                    FloatingTextManager.Instance.ShowIcon(drop.item.artwork, "", Vector3.zero, true);
                }
            }
        }
    }

    void PrepareCardForFight(CardController card, out Vector3 pos, out Vector3 scale, out int bgOrder, out int iconOrder, out int textOrder)
    {
        pos = card.transform.position;
        scale = card.transform.localScale;

        if (card.backgroundRenderer != null)
        {
            bgOrder = card.backgroundRenderer.sortingOrder;
            card.backgroundRenderer.sortingOrder = 100;
        }
        else { bgOrder = 0; }

        if (card.iconRenderer != null)
        {
            iconOrder = card.iconRenderer.sortingOrder;
            card.iconRenderer.sortingOrder = 101;
        }
        else { iconOrder = 1; }

        if (card.nameText != null)
        {
            textOrder = card.nameText.sortingOrder;
            card.nameText.sortingOrder = 102;
        }
        else { textOrder = 2; }
    }

    void ResetCardLayer(CardController card, int oldBgOrder, int oldIconOrder, int oldTextOrder)
    {
        if (card.backgroundRenderer != null) card.backgroundRenderer.sortingOrder = oldBgOrder;
        if (card.iconRenderer != null) card.iconRenderer.sortingOrder = oldIconOrder;
        if (card.nameText != null) card.nameText.sortingOrder = oldTextOrder;
    }
}