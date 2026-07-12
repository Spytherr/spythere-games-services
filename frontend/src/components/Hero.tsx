import { useState, useEffect } from 'react'

function Hero() {
  const [visible, setVisible] = useState(false)

  useEffect(() => {
    const timer = setTimeout(() => setVisible(true), 100)
    return () => clearTimeout(timer)
  }, [])

  return (
    <section className="flex flex-col items-center justify-center min-h-screen px-4 ">
      <img
        src="/logo.png"
        alt="Spythere Games"
        className={`logo w-80 h-50 mb-1 transition-all duration-700 ${
          visible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'
        }`}
      />
      {/* <h1 className="text-5xl font-bold mb-4">Spythere Games</h1> */}
      {/* <p
        className={`text-5xl text-[var(--text)] mb-8 py-7 transition-all duration-700 delay-200 ${
          visible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'
        }`}
      >
        Independent game developer
      </p> */}
      <div
        className={`flex gap-4 transition-all duration-700 delay-400 ${
          visible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'
        }`}
      >
        <span
          className="px-6 py-3 bg-red-600 text-white pixel-outline shadow-md opacity-40 cursor-not-allowed select-none"
        >
          YouTube
        </span>
        <a
          href="https://github.com/Spytherr"
          target="_blank"
          className="px-6 py-3 bg-gray-800 text-white pixel-outline shadow-md hover:bg-gray-900 hover:-translate-y-1 hover:shadow-lg transition-all"
        >
          GitHub
        </a>
      </div>
    </section>
  )
}
 
export default Hero