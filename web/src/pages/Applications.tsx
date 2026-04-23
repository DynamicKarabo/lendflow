import { useState } from "react"
import { FileText, Search } from "lucide-react"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Progress } from "@/components/ui/progress"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { useApplications } from "@/hooks/useApi"

function formatZAR(amount: number) {
  return new Intl.NumberFormat("en-ZA", {
    style: "currency",
    currency: "ZAR",
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(amount)
}

function StatusBadge({ status }: { status: string }) {
  const variantMap: Record<string, "success" | "warning" | "destructive" | "info" | "secondary"> = {
    Approved: "success",
    Rejected: "destructive",
    UnderReview: "warning",
    Submitted: "info",
    Draft: "secondary",
    Cancelled: "secondary",
  }
  return <Badge variant={variantMap[status] || "secondary"}>{status}</Badge>
}

function CreditScoreIndicator({ score }: { score?: number }) {
  if (!score) return <span className="text-muted-foreground text-sm">—</span>
  const variant = score >= 650 ? "success" : score >= 550 ? "warning" : "destructive"
  return (
    <div className="flex items-center gap-2">
      <span className="text-sm font-medium">{score}</span>
      <Progress value={score} max={850} variant={variant} className="w-16 h-1.5" />
    </div>
  )
}

export default function Applications() {
  const [filter, setFilter] = useState("all")
  const [search, setSearch] = useState("")
  const { data: applications, loading } = useApplications(filter === "all" ? undefined : filter)

  const filtered = applications.filter(
    (a) =>
      a.applicantName?.toLowerCase().includes(search.toLowerCase()) ||
      a.purpose?.toLowerCase().includes(search.toLowerCase())
  )

  return (
    <div className="space-y-6">
      <div className="flex items-end justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Applications</h1>
          <p className="text-muted-foreground mt-1">Review and process loan applications</p>
        </div>
      </div>

      <Card className="border-border/50">
        <CardHeader className="pb-4">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <CardTitle>All Applications</CardTitle>
              <CardDescription>{filtered.length} applications found</CardDescription>
            </div>
            <div className="relative">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="Search applications..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="pl-9 w-[260px]"
              />
            </div>
          </div>
          <Tabs value={filter} onValueChange={setFilter} className="mt-4">
            <TabsList>
              <TabsTrigger value="all">All</TabsTrigger>
              <TabsTrigger value="Draft">Draft</TabsTrigger>
              <TabsTrigger value="Submitted">Submitted</TabsTrigger>
              <TabsTrigger value="UnderReview">Review</TabsTrigger>
              <TabsTrigger value="Approved">Approved</TabsTrigger>
              <TabsTrigger value="Rejected">Rejected</TabsTrigger>
            </TabsList>
          </Tabs>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow className="hover:bg-transparent">
                <TableHead>Applicant</TableHead>
                <TableHead>Amount</TableHead>
                <TableHead>Term</TableHead>
                <TableHead>Purpose</TableHead>
                <TableHead>Credit Score</TableHead>
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
                    <FileText className="mx-auto h-10 w-10 mb-3 opacity-30" />
                    No applications found
                  </TableCell>
                </TableRow>
              ) : (
                filtered.map((app) => (
                  <TableRow key={app.id}>
                    <TableCell>
                      <div className="flex items-center gap-3">
                        <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center text-primary text-xs font-semibold">
                          {app.applicantName?.charAt(0)}
                        </div>
                        <div>
                          <p className="text-sm font-medium">{app.applicantName}</p>
                          <p className="text-xs text-muted-foreground">{app.id.slice(0, 8)}</p>
                        </div>
                      </div>
                    </TableCell>
                    <TableCell className="font-medium">{formatZAR(app.requestedAmount)}</TableCell>
                    <TableCell>{app.requestedTermMonths} mo</TableCell>
                    <TableCell className="text-muted-foreground">{app.purpose}</TableCell>
                    <TableCell>
                      <CreditScoreIndicator score={app.creditScore} />
                    </TableCell>
                    <TableCell>
                      <StatusBadge status={app.status} />
                    </TableCell>
                    <TableCell className="text-right">
                      <Button variant="ghost" size="sm">
                        Review
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
