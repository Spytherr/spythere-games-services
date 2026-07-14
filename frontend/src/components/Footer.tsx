import { useInView } from '../hooks/useInView'

function Footer() {
  const { ref, inView } = useInView<HTMLElement>()

  return (
    <footer ref={ref} className={`py-12 px-4 text-center transition-all duration-500 ${
      inView ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'
    }`}>
      <p className="text-2xl text-[var(--text)] mb-2">
        Contact:
      </p>
      <a
        href="mailto:spytherr@gmail.com"
        className="text-3xl text-[var(--accent)] hover:underline"
      >
        spytherr@gmail.com
      </a>
      <p className="text-sm text-[var(--text)] opacity-50 mt-8">
        © {new Date().getFullYear()} Spythere Games. All rights reserved.
      </p>
    </footer>
  )
}

export default Footer
