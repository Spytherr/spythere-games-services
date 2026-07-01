import { useTheme } from '../hooks/useTheme'

function ThemeToggle() {
  const { dark, toggle } = useTheme()

  return (
    <button
      onClick={toggle}
      className="fixed top-4 right-4 z-50 w-12 h-12 pixel-outline bg-gray-200 dark:bg-gray-800 shadow-md hover:shadow-lg hover:-translate-y-1 transition-all flex items-center justify-center text-2xl"
      aria-label="Toggle theme"
    >
      {dark ? '☀️' : '🌙'}
    </button>
  )
}

export default ThemeToggle
