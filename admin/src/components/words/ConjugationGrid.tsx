// DE (3 zaman × 6 kişi = 18 hücre) ve TR (5 zaman × 6 kişi = 30 hücre) çekim tabloları AYNI
// şekle sahip (Record<tense, Record<person, string>>) — bu yüzden tek, parametreli bir grid.
interface ConjugationGridProps<TTense extends string, TPerson extends string> {
  tenses: readonly { key: TTense; label: string }[]
  persons: readonly { key: TPerson; label: string }[]
  value: Partial<Record<TTense, Partial<Record<TPerson, string>>>> | undefined
  onChange: (value: Partial<Record<TTense, Partial<Record<TPerson, string>>>>) => void
}

export function ConjugationGrid<TTense extends string, TPerson extends string>({
  tenses,
  persons,
  value,
  onChange,
}: ConjugationGridProps<TTense, TPerson>) {
  const setCell = (tense: TTense, person: TPerson, text: string) => {
    onChange({
      ...value,
      [tense]: { ...value?.[tense], [person]: text },
    })
  }

  return (
    <div className="overflow-x-auto rounded-control border border-border">
      <table className="w-full text-sm">
        <thead>
          <tr className="bg-background">
            <th className="p-2 text-left font-medium text-muted">&nbsp;</th>
            {persons.map((person) => (
              <th key={person.key} className="p-2 text-left font-medium text-muted">
                {person.label}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {tenses.map((tense) => (
            <tr key={tense.key} className="border-t border-border">
              <td className="p-2 font-medium text-text">{tense.label}</td>
              {persons.map((person) => (
                <td key={person.key} className="p-1">
                  <input
                    type="text"
                    value={value?.[tense.key]?.[person.key] ?? ''}
                    onChange={(e) => setCell(tense.key, person.key, e.target.value)}
                    className="w-full rounded-control border border-border bg-background px-2 py-1 text-sm text-text"
                  />
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
