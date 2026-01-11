{{/*
Expand the name of the chart.
*/}}
{{- define "appointments-app.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
*/}}
{{- define "appointments-app.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "appointments-app.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "appointments-app.labels" -}}
helm.sh/chart: {{ include "appointments-app.chart" . }}
{{ include "appointments-app.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "appointments-app.selectorLabels" -}}
app.kubernetes.io/name: {{ include "appointments-app.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "appointments-app.serviceAccountName" -}}
{{- if .Values.serviceAccount.create -}}
{{- default (include "appointments-app.fullname" .) .Values.serviceAccount.name }}
{{- else -}}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}

{{/*
Create PostgreSQL name
*/}}
{{- define "appointments-app.postgresql.name" -}}
{{- printf "%s-postgresql" (include "appointments-app.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create PostgreSQL labels
*/}}
{{- define "appointments-app.postgresql.labels" -}}
helm.sh/chart: {{ include "appointments-app.chart" . }}
{{ include "appointments-app.postgresql.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Create PostgreSQL selector labels
*/}}
{{- define "appointments-app.postgresql.selectorLabels" -}}
app.kubernetes.io/name: {{ include "appointments-app.postgresql.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Create service name
*/}}
{{- define "appointments-app.service.name" -}}
{{- printf "%s-%s" (include "appointments-app.fullname" .context) .name | lower | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create service labels
*/}}
{{- define "appointments-app.service.labels" -}}
helm.sh/chart: {{ include "appointments-app.chart" .context }}
{{ include "appointments-app.service.selectorLabels" . }}
{{- if .context.Chart.AppVersion }}
app.kubernetes.io/version: {{ .context.Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .context.Release.Service }}
{{- end }}

{{/*
Create service selector labels
*/}}
{{- define "appointments-app.service.selectorLabels" -}}
app.kubernetes.io/name: {{ include "appointments-app.service.name" . }}
app.kubernetes.io/instance: {{ .context.Release.Name }}
app.kubernetes.io/component: {{ .name }}
{{- end }}

{{/*
Create database connection string
*/}}
{{- define "appointments-app.db.connectionString" -}}
{{- if .context.Values.postgresql.external.enabled -}}
Host={{ .context.Values.postgresql.external.host }};Port={{ .context.Values.postgresql.external.port }};Database={{ .database.name }};Username={{ .database.user }};Password={{ .database.password }};SslMode={{ .context.Values.postgresql.external.sslMode }}
{{- else -}}
Host={{ include "appointments-app.postgresql.name" .context }};Port={{ .context.Values.postgresql.port }};Database={{ .database.name }};Username={{ .database.user }};Password={{ .database.password }}
{{- end }}
{{- end }}

{{/*
Create migration connection string
*/}}
{{- define "appointments-app.db.migrationConnectionString" -}}
{{- if .context.Values.postgresql.external.enabled -}}
Host={{ .context.Values.postgresql.external.host }};Port={{ .context.Values.postgresql.external.port }};Database={{ .database.name }};Username={{ .context.Values.postgresql.external.adminUsername }};Password={{ .context.Values.postgresql.admin.password }};SslMode={{ .context.Values.postgresql.external.sslMode }}
{{- else -}}
Host={{ include "appointments-app.postgresql.name" .context }};Port={{ .context.Values.postgresql.port }};Database={{ .database.name }};Username={{ .context.Values.postgresql.admin.username }};Password={{ .context.Values.postgresql.admin.password }}
{{- end }}
{{- end }}

{{/*
Create secret name
*/}}
{{- define "appointments-app.secret.name" -}}
{{- printf "%s-%s-db-secret" (include "appointments-app.fullname" .context) .name | lower | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create migration job name
*/}}
{{- define "appointments-app.migration.name" -}}
{{- printf "migration-%s-job" .name | trunc 63 | trimSuffix "-" }}
{{- end }}