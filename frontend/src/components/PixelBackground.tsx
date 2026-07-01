import { useEffect, useRef } from 'react'

function PixelBackground() {
  const canvasRef = useRef<HTMLCanvasElement>(null)

  useEffect(() => {
    const canvas = canvasRef.current
    if (!canvas) return
    const ctx = canvas.getContext('2d')
    if (!ctx) return

    const pixelSize = 25
    const segments = 8
    const ringDistances = [6, 13, 24, 36, 50, 70]
    const splitIndexes = [6]

    let cols: number, rows: number
    let pixels: GridPixel[] = []
    let frame = 0
    let centerCol: number, centerRow: number
    let animationId: number
    let regenerateTimer: ReturnType<typeof setTimeout> | null = null

    class GridPixel {
      col: number
      row: number
      alpha: number
      state: string
      delay: number
      appearSpeed = 0.02
      disappearSpeed = 0
      lifeTime = Infinity
      lifeCounter = 0

      constructor(col: number, row: number, delay: number) {
        this.col = col
        this.row = row
        this.alpha = 0
        this.state = 'waiting'
        this.delay = delay + Math.random() * 5
      }

      update() {
        if (this.state === 'waiting' && frame > this.delay) this.state = 'appearing'

        if (this.state === 'appearing') {
          this.alpha += this.appearSpeed
          if (this.alpha >= 1) { this.alpha = 1; this.state = 'visible' }
        } else if (this.state === 'visible') {
          // nie znika
        }
      }

      draw(color: string) {
        if (this.alpha > 0) {
          ctx!.fillStyle = color.replace('ALPHA', String(this.alpha * 0.08))
          ctx!.fillRect(this.col * pixelSize, this.row * pixelSize, pixelSize, pixelSize)
        }
      }
    }

    function getBresenhamLine(c1: number, r1: number, c2: number, r2: number) {
      const points: { col: number; row: number }[] = []
      let dx = Math.abs(c2 - c1)
      let dy = Math.abs(r2 - r1)
      let sx = (c1 < c2) ? 1 : -1
      let sy = (r1 < r2) ? 1 : -1
      let err = dx - dy
      let currentC = c1
      let currentR = r1

      while (true) {
        points.push({ col: currentC, row: currentR })
        if (currentC === c2 && currentR === r2) break
        let e2 = 2 * err
        if (e2 > -dy) { err -= dy; currentC += sx }
        if (e2 < dx) { err += dx; currentR += sy }
      }
      return points
    }

    function generateNewWeb() {
      pixels = []
      frame = 0

      const maxRing = ringDistances[ringDistances.length - 1]
      const margin = Math.min(maxRing + 2, Math.floor(cols / 3), Math.floor(rows / 3))
      const safeCols = Math.max(cols - margin * 2, 1)
      const safeRows = Math.max(rows - margin * 2, 1)
      centerCol = Math.floor(margin + Math.random() * safeCols)
      centerRow = Math.floor(margin + Math.random() * safeRows)

      const pixelMap = new Map<string, GridPixel>()

      const addLine = (c1: number, r1: number, c2: number, r2: number) => {
        const linePoints = getBresenhamLine(c1, r1, c2, r2)
        linePoints.forEach((p) => {
          const key = `${p.col},${p.row}`
          if (!pixelMap.has(key)) {
            const distFromCenter = Math.sqrt((p.col - centerCol) ** 2 + (p.row - centerRow) ** 2)
            pixelMap.set(key, new GridPixel(p.col, p.row, distFromCenter * 2))
          }
        })
      }

      const nodes: { col: number; row: number }[][] = []
      nodes[0] = [{ col: centerCol, row: centerRow }]

      for (let r = 1; r < ringDistances.length; r++) {
        nodes[r] = []
        const dist = ringDistances[r]
        const splitMultiplier = splitIndexes.reduce((multiplier, splitIndex) => {
          return r > splitIndex ? multiplier * 2 : multiplier
        }, 1)
        const currentSegments = segments * splitMultiplier
        const twist = Math.PI / segments

        for (let s = 0; s < currentSegments; s++) {
          const angle = (s / currentSegments) * Math.PI * 2 + twist
          const nodeCol = centerCol + Math.round(Math.cos(angle) * dist)
          const nodeRow = centerRow + Math.round(Math.sin(angle) * dist)
          nodes[r].push({ col: nodeCol, row: nodeRow })
        }
      }

      for (let r = 0; r < ringDistances.length - 1; r++) {
        const currentNodes = nodes[r]
        const nextNodes = nodes[r + 1]
        const splitRatio = nextNodes.length / currentNodes.length

        for (let s = 0; s < currentNodes.length; s++) {
          const startNode = currentNodes[s]
          for (let i = 0; i < splitRatio; i++) {
            const targetNode = nextNodes[s * splitRatio + i]
            addLine(startNode.col, startNode.row, targetNode.col, targetNode.row)
          }
        }
      }

      for (let r = 1; r < ringDistances.length; r++) {
        let segCount = nodes[r].length
        for (let s = 0; s < segCount; s++) {
          const nextS = (s + 1) % segCount
          addLine(nodes[r][s].col, nodes[r][s].row, nodes[r][nextS].col, nodes[r][nextS].row)
        }
      }

      pixels = Array.from(pixelMap.values())
    }

    function getColor(): string {
      const isDark = document.documentElement.classList.contains('dark')
      if (isDark) {
        return 'rgba(255, 255, 255, ALPHA)'
      }
      return 'rgba(0, 0, 0, ALPHA)'
    }

    function animate() {
      ctx!.clearRect(0, 0, canvas!.width, canvas!.height)
      frame++

      const color = getColor()

      pixels.forEach(p => {
        p.update()
        p.draw(color)
      })

      animationId = requestAnimationFrame(animate)
    }

    let resizeTimer: ReturnType<typeof setTimeout> | null = null
    let isResizing = false
    let prevWidth = window.innerWidth
    let prevHeight = window.innerHeight

    const OVERSCAN = 100

    function resizeCanvas(regenerate: boolean) {
      const w = window.innerWidth
      const h = window.innerHeight
      canvas!.width = w
      canvas!.height = h + OVERSCAN * 2
      canvas!.style.width = w + 'px'
      canvas!.style.height = (h + OVERSCAN * 2) + 'px'
      canvas!.style.top = -OVERSCAN + 'px'
      ctx!.imageSmoothingEnabled = false
      cols = Math.floor(canvas!.width / pixelSize)
      rows = Math.floor(canvas!.height / pixelSize)
      if (regenerate) {
        generateNewWeb()
        if (regenerateTimer) clearTimeout(regenerateTimer)
        cancelAnimationFrame(animationId)
        animate()
      }
      isResizing = false
    }

    const resizeHandler = () => {
      const widthChanged = window.innerWidth !== prevWidth
      const heightDiff = Math.abs(window.innerHeight - prevHeight)
      const significantChange = widthChanged || heightDiff > 200

      if (significantChange) {
        if (!isResizing) {
          isResizing = true
          cancelAnimationFrame(animationId)
          ctx!.clearRect(0, 0, canvas!.width, canvas!.height)
        }
        if (resizeTimer) clearTimeout(resizeTimer)
        resizeTimer = setTimeout(() => {
          prevWidth = window.innerWidth
          prevHeight = window.innerHeight
          resizeCanvas(true)
        }, 200)
      } else {
        prevHeight = window.innerHeight
      }
    }
    window.addEventListener('resize', resizeHandler)
    resizeCanvas(true)

    return () => {
      window.removeEventListener('resize', resizeHandler)
      cancelAnimationFrame(animationId)
      if (regenerateTimer) clearTimeout(regenerateTimer)
      if (resizeTimer) clearTimeout(resizeTimer)
    }
  }, [])

  return (
    <canvas
      ref={canvasRef}
      className="fixed left-0 pointer-events-none"
      style={{ zIndex: 0 }}
    />
  )
}

export default PixelBackground
