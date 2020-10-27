using EnemyController.Interfaces;
using PlayerController.Interfaces;
using PoolManager;
using PoolManager.Interfaces;
using UnityEngine;

namespace EnemyController
{
    public class EnemyController : MonoBehaviour, IEnemyController
    {
        [SerializeField] private GameObject asteroidPrefab;
        [SerializeField] private GameObject ufoPrefab;

        [SerializeField] private float spawnPointOffset;

        [SerializeField] private float minStartForce;
        [SerializeField] private float maxStartForce;
        private SpawnPointGenerator _spawnPointGenerator;
        private StartForceGenerator _startForceGenerator;
        private float _time = 0f;
        private const float SpawnDelay = 0.3f;

        private MapCoordinates _mapCoordinates;

        private PoolManager<Asteroid> _asteroidManager;
        private PoolManager<Ufo> _ufosManager;
        private ITarget _target;

        public void Initialize(MapCoordinates mapCoordinates, ITarget target)
        {
            _time = 0f;
            _target = target;
            _startForceGenerator = new StartForceGenerator(mapCoordinates, minStartForce, maxStartForce);
            _spawnPointGenerator = new SpawnPointGenerator(mapCoordinates, spawnPointOffset);
            _mapCoordinates = mapCoordinates;

            _ufosManager = new PoolManager<Ufo>(CreateUfo, 3);
            _asteroidManager = new PoolManager<Asteroid>(CreateAsteroid, 1);
        }

        private Ufo CreateUfo(bool arg)
        {
            var ufoObject = Instantiate(ufoPrefab);
            var ufo = ufoObject.GetComponent<Ufo>();
            ufo.Initialize(_spawnPointGenerator, _target);

            ufoObject.SetActive(false);
            return ufo;
        }


        //TODO: it must be in another class, I think
        private Asteroid CreateAsteroid(bool isActive)
        {
            var asteroidObject = Instantiate(asteroidPrefab);
            var asteroid = asteroidObject.GetComponent<Asteroid>();
            asteroid.Initialize(_mapCoordinates, _spawnPointGenerator, _startForceGenerator);

            asteroidObject.SetActive(false);
            return asteroid;
        }

        public void DirectUpdate()
        {
            _asteroidManager.UpdateEnabledObjects();
            _ufosManager.UpdateEnabledObjects();

            if (!IsNeedToSpawnEnemy())
            {
                return;
            }

            SpawnEnemy();
        }

        //TODO: add enemy spawn chance
        private void SpawnEnemy()
        {
            var enemy = GetEnemyForSpawn();
            enemy.Activate();
        }

        private IPoolObject GetEnemyForSpawn()
        {
            // var flag = Random.Range(0f, 1f) > 0.5f;
            // if (flag)
            // {
                // return _asteroidManager.GetPoolObject();
            // }

            return _ufosManager.GetPoolObject();
        }

        private bool IsNeedToSpawnEnemy()
        {
            if(_time < SpawnDelay)
            {
                _time += Time.deltaTime;
                return false;
            }

            _time = 0f;
            return true;
        }
    }
}