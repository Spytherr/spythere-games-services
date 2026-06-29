import { useState, useEffect } from 'react'
import type { Game } from '../types'
import { fetchGames } from '../api/client'
import { getGameDescription } from '../gameData'

function GamesSection() {
  // useState — jak pole w klasie C#, przechowuje stan komponentu
  const [games, setGames] = useState<Game[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // useEffect — wykonuje się po wyrenderowaniu komponentu
  // [] na końcu = uruchom TYLKO RAZ przy pierwszym renderze
  useEffect(() => {
    fetchGames()
      .then((data) => setGames(data))
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <p className="text-center py-12">Loading games...</p>
  if (error) return <p className="text-center py-12 text-red-500">{error}</p>

  return (
    <section className="py-16 px-4">
      <h2 className="text-6xl text-center mb-8">Games</h2>
      <div className="grid grid-cols-1 md:grid-cols-1 gap-6 max-w-5xl mx-auto">
        {games.map((game) => (
          <div
            key={game.Id}
            className="flex  gap-6 border-4 border-gray-200 p-6 hover:shadow-lg transition"
          >
            <div className="flex-1 text-left">
              <div className="flex items-center gap-3 mb-8">
                <img
                  src={`/${game.Key}/icon.png`}
                  alt={game.Name}
                  className="w-16 h-16 object-cover outline-3 outline-white"
                />
                <h3 className="text-5xl ">{game.Name}</h3>
              </div>
              <p className="text-gray-300 w-full mb-4">{getGameDescription(game.Key)}</p>
              <a
                href="https://play.google.com/store"
                target="_blank"
                rel="noopener noreferrer"
                className="inline-block px-6 py-3 mt-4 bg-green-500 text-white outline-3 hover:bg-green-600 transition"
              >
                Google Play Store
              </a>
            </div>
            <img
              src={`/${game.Key}/screenshot.png`}
              alt={game.Name}
              className="w-32 h-62 object-cover outline-3 outline-white"
            />
          </div>
        ))}
      </div>
    </section>
  )
}

export default GamesSection