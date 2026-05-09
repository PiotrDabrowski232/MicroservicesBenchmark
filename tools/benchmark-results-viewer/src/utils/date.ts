type DateTimeFormatOptions = Intl.DateTimeFormatOptions

export function formatDateTime(value: string | null, options: DateTimeFormatOptions) {
  if (!value) {
    return 'Unknown start time'
  }

  const timestamp = Date.parse(value)
  if (Number.isNaN(timestamp)) {
    return value
  }

  return new Intl.DateTimeFormat(undefined, options).format(new Date(timestamp))
}
