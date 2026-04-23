import { BrowserRouter, Routes, Route } from "react-router-dom"
import { AppShell } from "@/components/layout/AppShell"
import Dashboard from "@/pages/Dashboard"
import Loans from "@/pages/Loans"
import LoanDetail from "@/pages/LoanDetail"
import Applications from "@/pages/Applications"
import Applicants from "@/pages/Applicants"

function App() {
  return (
    <BrowserRouter>
      <AppShell>
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/loans" element={<Loans />} />
          <Route path="/loans/:id" element={<LoanDetail />} />
          <Route path="/applications" element={<Applications />} />
          <Route path="/applicants" element={<Applicants />} />
        </Routes>
      </AppShell>
    </BrowserRouter>
  )
}

export default App
