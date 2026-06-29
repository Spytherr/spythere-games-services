function Hero() {
  return (
    <section className="flex flex-col items-center justify-center min-h-[60vh] px-4 ">
      <img src="/logo.png" alt="Spythere Games" className="w-32 h-32 mb-6" />
      <h1 className="text-5xl font-bold mb-4">Spythere Games</h1>
      <p className="text-lg text-gray-500 mb-8 py-7">Independent game developer</p>
      <div className="flex gap-4">
        <a
          href="https://youtube.com/@yourchannel"
          target="_blank"
          className="px-6 py-3 bg-red-600 text-white outline-3 hover:bg-red-700 transition"
        >
          YouTube
        </a>
        <a
          href="https://github.com/yourusername"
          target="_blank"
          className="px-6 py-3 bg-gray-800 text-white outline-3 hover:bg-gray-900 transition"
        >
          GitHub
        </a>
      </div>
    </section>
  )
}
 
export default Hero