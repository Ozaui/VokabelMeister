import { X } from 'lucide-react'

interface SidebarProps {
  isOpen: boolean
  onClose: () => void
}

function Sidebar({ isOpen, onClose }: SidebarProps) {
  return (
    <>
      {isOpen && (
        <div className="fixed inset-0 z-30 bg-black/40 md:hidden" onClick={onClose} aria-hidden="true" />
      )}
      <aside
        className={`fixed inset-y-0 left-0 z-40 w-56 shrink-0 border-r border-border bg-surface p-4 transition-transform duration-200 md:static md:translate-x-0 ${
          isOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        <button
          type="button"
          aria-label="Menüyü kapat"
          onClick={onClose}
          className="mb-3 text-text-secondary md:hidden"
        >
          <X size={20} />
        </button>
        <p className="text-sm text-text-secondary">
          Navigasyon linkleri B-03&apos;te (Kelime Yönetimi) eklenecek.
        </p>
      </aside>
    </>
  )
}

export default Sidebar
