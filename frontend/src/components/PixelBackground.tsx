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

    const frameDuration = 1000 / 60
    const webVisibleFrames = 300
    const webFadeSpeed = 0.01
    const distantWebChance = 0.4
    const minimumWebDistanceRatio = 0.35

    let cols: number, rows: number
    let animationId: number
    let previousAnimationTimestamp: number | null = null

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

      update(webFrame: number) {
        if (this.state === 'waiting' && webFrame > this.delay) this.state = 'appearing'

        if (this.state === 'appearing') {
          this.alpha += this.appearSpeed
          if (this.alpha >= 1) { this.alpha = 1; this.state = 'visible' }
        } else if (this.state === 'visible') {
          // nie znika
        }
      }

      draw(color: string, webOpacity: number) {
        if (this.alpha > 0) {
          ctx!.fillStyle = color.replace('ALPHA', String(this.alpha * webOpacity * 0.08))
          ctx!.fillRect(this.col * pixelSize, this.row * pixelSize, pixelSize, pixelSize)
        }
      }
    }

    type Web = {
      centerCol: number
      centerRow: number
      pixels: GridPixel[]
      age: number
      opacity: number
      isFading: boolean
    }

    let activeWebs: Web[] = []

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

    function getWebCenter(previousWeb?: Web) {
      const maxRing = ringDistances[ringDistances.length - 1]
      const margin = Math.min(maxRing + 2, Math.floor(cols / 3), Math.floor(rows / 3))
      const safeCols = Math.max(cols - margin * 2, 1)
      const safeRows = Math.max(rows - margin * 2, 1)
      const getCandidate = () => ({
        col: Math.floor(margin + Math.random() * safeCols),
        row: Math.floor(margin + Math.random() * safeRows),
      })

      if (!previousWeb) return getCandidate()

      const maxDistance = Math.hypot(safeCols - 1, safeRows - 1)
      const minimumDistance = Math.min(
        Math.max(maxDistance * minimumWebDistanceRatio, 12),
        maxDistance,
      )
      const candidates: Array<{ col: number; row: number; distance: number }> = []
      let furthestCandidate = getCandidate()
      let furthestDistance = 0

      for (let attempt = 0; attempt < 24; attempt++) {
        const candidate = getCandidate()
        const distance = Math.hypot(
          candidate.col - previousWeb.centerCol,
          candidate.row - previousWeb.centerRow,
        )
        if (distance > furthestDistance) {
          furthestCandidate = candidate
          furthestDistance = distance
        }
        if (distance >= minimumDistance) candidates.push({ ...candidate, distance })
      }

      if (!candidates.length) return furthestCandidate

      candidates.sort((a, b) => a.distance - b.distance)
      if (Math.random() < distantWebChance) {
        const candidate = candidates[candidates.length - 1]
        return { col: candidate.col, row: candidate.row }
      }

      const nearbyCandidateCount = Math.max(1, Math.ceil(candidates.length / 3))
      const candidate = candidates[Math.floor(Math.random() * nearbyCandidateCount)]
      return { col: candidate.col, row: candidate.row }
    }

    function generateNewWeb(previousWeb?: Web): Web {
      const { col: centerCol, row: centerRow } = getWebCenter(previousWeb)
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

      return {
        centerCol,
        centerRow,
        pixels: Array.from(pixelMap.values()),
        age: 0,
        opacity: 1,
        isFading: false,
      }
    }

    function getColor(): string {
      const isDark = document.documentElement.classList.contains('dark')
      if (isDark) {
        return 'rgba(255, 255, 255, ALPHA)'
      }
      return 'rgba(0, 0, 0, ALPHA)'
    }

    function animate(timestamp = performance.now()) {
      const elapsedFrames = previousAnimationTimestamp === null
        ? 1
        : Math.min((timestamp - previousAnimationTimestamp) / frameDuration, 3)
      previousAnimationTimestamp = timestamp

      ctx!.clearRect(0, 0, canvas!.width, canvas!.height)

      const color = getColor()
      const fadedWebs: Web[] = []

      activeWebs.forEach((web) => {
        web.age += elapsedFrames
        if (!web.isFading && web.age >= webVisibleFrames) web.isFading = true
        if (web.isFading) {
          web.opacity = Math.max(0, web.opacity - webFadeSpeed * elapsedFrames)
          if (web.opacity === 0) fadedWebs.push(web)
        }

        web.pixels.forEach((pixel) => {
          pixel.update(web.age)
          pixel.draw(color, web.opacity)
        })
      })

      activeWebs = activeWebs.filter(web => web.opacity > 0)
      const previousWeb = fadedWebs[fadedWebs.length - 1]
      if (activeWebs.length === 0 && previousWeb) activeWebs.push(generateNewWeb(previousWeb))
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
        activeWebs = [generateNewWeb()]
        previousAnimationTimestamp = null
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
