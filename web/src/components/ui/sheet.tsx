import * as React from "react"
import { X } from "lucide-react"
import { cn } from "@/lib/utils"

const SheetContext = React.createContext<{ open: boolean; setOpen: (v: boolean) => void }>(
  { open: false, setOpen: () => {} }
)

function Sheet({ children, open: controlledOpen, onOpenChange }: {
  children: React.ReactNode;
  open?: boolean;
  onOpenChange?: (open: boolean) => void;
}) {
  const [uncontrolledOpen, setUncontrolledOpen] = React.useState(false)
  const open = controlledOpen !== undefined ? controlledOpen : uncontrolledOpen
  const setOpen = (value: boolean) => {
    setUncontrolledOpen(value)
    onOpenChange?.(value)
  }
  return (
    <SheetContext.Provider value={{ open, setOpen }}>
      {children}
    </SheetContext.Provider>
  )
}

function SheetTrigger({ children, asChild, className }: {
  children: React.ReactNode;
  asChild?: boolean;
  className?: string;
}) {
  const { setOpen } = React.useContext(SheetContext)
  if (asChild && React.isValidElement(children)) {
    return (
      <span className={className} onClick={() => setOpen(true)}>
        {React.cloneElement(children as React.ReactElement, {})}
      </span>
    )
  }
  return (
    <button className={className} onClick={() => setOpen(true)}>
      {children}
    </button>
  )
}

function SheetContent({ children, side = "right", className }: {
  children: React.ReactNode;
  side?: "left" | "right" | "top" | "bottom";
  className?: string;
}) {
  const { open, setOpen } = React.useContext(SheetContext)
  const sideClasses = {
    left: "inset-y-0 left-0 h-full w-3/4 max-w-sm border-r data-[state=closed]:-translate-x-full",
    right: "inset-y-0 right-0 h-full w-3/4 max-w-sm border-l data-[state=closed]:translate-x-full",
    top: "inset-x-0 top-0 w-full h-auto max-h-[50vh] border-b data-[state=closed]:-translate-y-full",
    bottom: "inset-x-0 bottom-0 w-full h-auto max-h-[50vh] border-t data-[state=closed]:translate-y-full",
  }
  return (
    <>
      <div
        className={cn(
          "fixed inset-0 z-50 bg-black/80 backdrop-blur-sm transition-all duration-300",
          open ? "opacity-100 pointer-events-auto" : "opacity-0 pointer-events-none"
        )}
        onClick={() => setOpen(false)}
      />
      <div
        className={cn(
          "fixed z-50 gap-4 bg-background p-6 shadow-lg transition-transform duration-300 ease-in-out",
          sideClasses[side],
          open ? "translate-x-0 translate-y-0" : "",
          className
        )}
      >
        <button
          onClick={() => setOpen(false)}
          className="absolute right-4 top-4 rounded-sm opacity-70 ring-offset-background transition-opacity hover:opacity-100 focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2"
        >
          <X className="h-4 w-4" />
          <span className="sr-only">Close</span>
        </button>
        {children}
      </div>
    </>
  )
}

export { Sheet, SheetTrigger, SheetContent }
