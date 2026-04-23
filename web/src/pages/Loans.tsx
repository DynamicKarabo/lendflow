import { useState } from "react"
import { Link } from "react-router-dom"
import { Banknote, Search, Eye } from "lucide-react"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { useLoans } from "@/hooks/useApi"

function formatZAR(amount: number) {
  return new Intl.NumberFormat("en-ZA", {
    style: "currency",
    currency: "ZAR",
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(amount)
}

function StatusBadge({ status }: { status: string }) {
  const variantMap: Record<string, "success" | "warning" | "destructive" | "secondary"> = {
    Active: "success",
    PendingDisbursement: "warning",
    Settled: "secondary",
    Defaulted: "destructive",
  }
  return <Badge variant={variantMap[status] || "secondary"}>{status}</Badge>
}

export default function Loans() {
  const [filter, setFilter] = useState("all")
  const [search, setSearch] = useState("")
  const { data: loans, loading } = useLoans(filter === "all" ? undefined : filter)

  const filtered = loans.filter(
    (l) =>
      l.applicantName?.toLowerCase().includes(search.toLowerCase())
  )

  return (
    <div className="space-y-6">
      <div className="flex items-end justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Loans</h1>
          <p className="text-muted-foreground mt-1">Manage and track all loan accounts</p>
        </div>
      </div>

      <Card className="border-border/50">
        <CardHeader className="pb-4">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <CardTitle>Loan Portfolio</CardTitle>
              <CardDescription>{filtered.length} loans found</CardDescription>
            </div>
            <div className="flex items-center gap-3">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  placeholder="Search loans..."
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  className="pl-9 w-[260px]"
                />
              </div>
            </div>
          </div>
          <Tabs value={filter} onValueChange={setFilter} className="mt-4">
            <TabsList>
              <TabsTrigger value="all">All</TabsTrigger>
              <TabsTrigger value="Active">Active</TabsTrigger>
              <TabsTrigger value="PendingDisbursement">Pending</TabsTrigger>
              <TabsTrigger value="Settled">Settled</TabsTrigger>
            </TabsList>
          </Tabs>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow className="hover:bg-transparent">
                <TableHead>Applicant</TableHead>
                <TableHead>Principal</TableHead>
                <TableHead>Rate</TableHead>
                <TableHead>Term</TableHead>
                <TableHead>Outstanding</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                Array.from({ length: 5 }).map((_, i) => (
                  <TableRow key={i}>
                    <TableCell colSpan={7}>
                      <div className="h-10 bg-muted animate-pulse rounded" />
                    </TableCell>
                  </TableRow>
                ))
              ) : filtered.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} className="text-center py-12 text-muted-foreground">
                    <Banknote className="mx-auto h-10 w-10 mb-3 opacity-30" />
                    No loans found
                  </TableCell>
                </TableRow>
              ) : (
                filtered.map((loan) => (
                  <TableRow key={loan.id}>
                    <TableCell>
                      <div className="flex items-center gap-3">
                        <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center text-primary text-xs font-semibold">
                          {loan.applicantName?.charAt(0)}
                        </div>
                        <div>
                          <p className="text-sm font-medium">{loan.applicantName}</p>
                          <p className="text-xs text-muted-foreground">{loan.id.slice(0, 8)}</p>
                        </div>
                      </div>
                    </TableCell>
                    <TableCell className="font-medium">{formatZAR(loan.principal)}</TableCell>
                    <TableCell>{(loan.interestRate * 100).toFixed(1)}%</TableCell>
                    <TableCell>{loan.termMonths} mo</TableCell>
                    <TableCell className={loan.outstandingBalance === 0 ? "text-muted-foreground" : "font-medium"}>
                      {formatZAR(loan.outstandingBalance)}
                    </TableCell>
                    <TableCell>
                      <StatusBadge status={loan.status} />
                    </TableCell>
                    <TableCell className="text-right">
                      <Button variant="ghost" size="sm">
                        <Link to={`/loans/${loan.id}`} className="flex items-center">
                          <Eye className="h-4 w-4 mr-1" />
                          View
                        </Link>
                      </Button>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  )
}
