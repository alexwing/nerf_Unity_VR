
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CubeBeatWar
{

    public class LevelCreator : MonoBehaviour
    {

        [SerializeField] private TMPro.TextMeshPro hitScoreText;
        [SerializeField] private TMPro.TextMeshPro TimeLapsedText;
        [SerializeField] private TMPro.TextMeshPro scoreText;

        public static LevelCreator instance;
        [Tooltip("Contains all the enemies")]
        public GameObject enemyContainer;


        [Tooltip("The enemy prefab")]
        public GameObject enemyPrefab;

        [Tooltip("The number of enemies to spawn")]
        public int enemiesInLevel = 10;
        [Tooltip("Distance between enemies")]
        public int enemiesDistanceBetween = 5;
        public float enemiesVelocity = 0.01f;
        [Tooltip("Time to wait before spawn new enemy")]
        public float enemiesCreationPeriod = 1f;

        [Header("Plane defines enemies creation positions limits")]
        [Tooltip("quad top left")]
        public Vector2 quadTopLeft;
        [Tooltip("quad bottom right")]
        public Vector2 quadBottomRight;
        [Header("Colors")]
        [Tooltip("Enemy colors")]
        public Color[] enemyColors;

        private int enemyCreationIndex = 0;
        private List<EnemyController> enemyList = new List<EnemyController>();

        private List<ScoreItem> scoreList = new List<ScoreItem>();

        private int enemiesDeaths = 0;

        public static bool gameIsActive = false;


        private float timeLapsed = 0.0f;

        private void Update()
        {
            if (gameIsActive)
            {
                timeLapsed += Time.deltaTime;
                TimeLapsedText.text = "Time: " + timeLapsed.ToString("F2");
            }

        }

        public void Awake()
        {
            instance = this;
            scoreText.text = "";
        }
        public void StartGame()
        {
            GameMenu.instance.hideMenu();

            if (gameIsActive)
            {
                StopGame();
            }
            SceneConfig.gameIsPaused = false;
            gameIsActive = true;
            timeLapsed = 0.0f;
            enemiesDeaths = 0;
            hitScoreText.text = "Hits: " + enemiesDeaths.ToString() + " / " + enemiesInLevel.ToString();
            StartCoroutine(CreateEnemies());
        }
        public void StopGame()
        {
            StopAllCoroutines();
            //remove all enemies
            foreach (EnemyController enemy in enemyList)
            {
                Destroy(enemy.gameObject);
            }
            enemyList.Clear();
            enemyCreationIndex = 0;
        }


        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            //PAINT QUAD
            Gizmos.DrawLine(new Vector3(quadTopLeft.x, 0, quadTopLeft.y), new Vector3(quadTopLeft.x, 0, quadBottomRight.y));
            Gizmos.DrawLine(new Vector3(quadTopLeft.x, 0, quadBottomRight.y), new Vector3(quadBottomRight.x, 0, quadBottomRight.y));
            Gizmos.DrawLine(new Vector3(quadBottomRight.x, 0, quadBottomRight.y), new Vector3(quadBottomRight.x, 0, quadTopLeft.y));
            Gizmos.DrawLine(new Vector3(quadBottomRight.x, 0, quadTopLeft.y), new Vector3(quadTopLeft.x, 0, quadTopLeft.y));
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(new Vector3(quadTopLeft.x, 0, quadTopLeft.y), 1f);
            Gizmos.DrawSphere(new Vector3(quadBottomRight.x, 0, quadBottomRight.y), 1f);
        }
        IEnumerator CreateEnemies()
        {

            while (true)
            {
                if (SceneConfig.gameIsPaused)
                {
                    yield return null;
                }
                else
                {

                    if (enemyCreationIndex < enemiesInLevel)
                    {

                        List<Vector3> enemyPositions = enemyList.ConvertAll(x => x.transform.position);

                        Vector4 quad = new Vector4(quadTopLeft.x, quadTopLeft.y, quadBottomRight.x, quadBottomRight.y);

                        // Random position in the terrain with a size limit and a distance limit between hills
                        Vector3 position = Utils.CreateRamdomPosition(quad, ref enemyPositions, enemiesDistanceBetween);

                        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity) as GameObject;
                        enemy.transform.parent = enemyContainer.transform;
                        enemy.name = "enemy" + enemyCreationIndex;
                        EnemyController enemyController = enemy.GetComponent<EnemyController>();
                        enemyList.Add(enemyController);
                        enemyController.velocity = enemiesVelocity;
                        enemyController.color = enemyColors[enemyCreationIndex % enemyColors.Length];
                        enemyController.appearEnemy();

                        enemyCreationIndex++;

                        //random time
                        float ramdomizeTime = Random.Range(0, enemiesCreationPeriod * 1000) / 1000;

                        yield return new WaitForSeconds(ramdomizeTime);

                    }
                    else
                    {
                        StopCoroutine(CreateEnemies());
                        yield return null;
                    }
                }
            }

        }

        public void enemyDeath(EnemyController enemyController)
        {
            enemiesDeaths++;
            hitScoreText.text = "Hits: " + enemiesDeaths.ToString() + "/" + enemiesInLevel.ToString();
            enemyList.Remove(enemyController);
            if (enemiesDeaths >= enemiesInLevel)
            {
                gameIsActive = false;
                GameMenu.instance.showMenu();
                ScoreItem scoreItem = new ScoreItem();
                scoreItem.hits = enemiesDeaths;
                scoreItem.enemies = enemiesInLevel;
                scoreItem.timeLapsed = timeLapsed;
                GameMenu.instance.showMenu();
            }

        }

        private void printScore()
        {   //limit array size to 4 last scores
            if (scoreList.Count > 4)
            {
                scoreList.RemoveRange(0, scoreList.Count - 4);
            }
            scoreText.text = "";
            foreach (ScoreItem score in scoreList)
            {
                scoreText.text += "Hits: " + score.hits.ToString() + " / " + score.enemies.ToString() + " Time: " + score.timeLapsed.ToString("F2") + "\n";
            }

        }

    }
}
