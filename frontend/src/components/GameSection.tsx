import { useState, useEffect } from 'react'
import type { Game } from '../types'
import { fetchGames } from '../api/client'
import { getGameDescription } from '../gameData'
import { useInView } from '../hooks/useInView'

function GamesSection() {
  const [games, setGames] = useState<Game[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const { ref, inView } = useInView<HTMLElement>()

  useEffect(() => {
    fetchGames()
      .then((data) => setGames(data))
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }, [])

  return (
    <section ref={ref} className={`py-16 px-4 transition-all duration-500 ${
      inView ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'
    }`}>
      {loading && <p className="text-center py-12">Loading games...</p>}
      {error && <p className="text-center py-12 text-red-500">{error}</p>}
      <h2 className="text-6xl text-center mb-8">Games</h2>
      <div className="grid grid-cols-1 md:grid-cols-1 gap-6 max-w-5xl mx-auto">
        {games.map((game) => (
          <div
            key={game.Id}
            className={`flex flex-col md:flex-row gap-6 pixel-outline p-6 bg-[var(--bg)]/80 backdrop-blur-sm hover:shadow-lg transition-all duration-500 ${
              inView ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'
            }`}
          >
            <div className="flex-1 min-w-0 text-left">
              <div className="flex items-center gap-3 mb-8">
                <img
                  src={`/${game.Key}/icon.png`}
                  alt={game.Name}
                  className="w-16 h-16 object-cover pixel-outline"
                />
                <h3 className="text-5xl ">{game.Name}</h3>
              </div>
              <p
                className={`text-[var(--text)] w-full mb-4 transition-all duration-500 delay-150 ${
                  inView ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'
                }`}
              >
                {getGameDescription(game.Key)}
              </p>
              <div
                className={`flex flex-wrap gap-4 mt-4 transition-all duration-500 delay-300 ${
                  inView ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'
                }`}
              >
                <a
                  href="https://play.google.com/store"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-block px-6 py-3 bg-green-500 text-white pixel-outline shadow-md hover:bg-green-600 hover:-translate-y-1 hover:shadow-lg transition-all"
                >
                  Google Play Store
                </a>
                <a
                  href="https://youtube.com/@yourchannel"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-block px-6 py-3 bg-red-600 text-white pixel-outline shadow-md hover:bg-red-700 hover:-translate-y-1 hover:shadow-lg transition-all"
                >
                  Youtube
                </a>
              </div>
            </div>
            <img
              src={`/${game.Key}/screenshot.png`}
              alt={game.Name}
              className="w-full md:w-40 h-48 md:h-84 object-cover pixel-outline shrink-0 mx-auto md:mx-0"
            />
          </div>
        ))}
      </div>
    </section>
  )
}

export default GamesSection