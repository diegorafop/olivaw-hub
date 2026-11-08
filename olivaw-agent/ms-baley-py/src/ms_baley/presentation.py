"""Apresentação do relatório de consulta, sem efeitos externos."""
import json


def review_text(report):
  if report.get("operation") != "update":
    return json.dumps(report, ensure_ascii=False, indent=2)
  lines = ["#source"]
  for source in report.get("source", []):
    lines.append(source["url"] + " [" + source["status"] + "]")
  lines.append("\n#discovery")
  for item in report.get("discovery", []):
    lines.append(item["url"] + " [não verificada; revisão do operador]")
  if not report.get("discovery"):
    lines.append("Nenhuma fonte candidata retornada; confira o resultado das buscas.")
  lines.append("\n#learn")
  for item in report.get("learn", []):
    lines.append(str(item["number"]) + " - " + item["action"])
    for evidence in item.get("evidence", []):
      lines.append("    " + evidence["url"])
  if not report.get("learn"):
    lines.append("Nenhum ponto novo sustentado pela consulta atual.")
  lines.append("\nResultado: " + report["status"] + "; nenhuma alteração aplicada.")
  for search in report.get("research", {}).get("searches", []):
    if search["status"] != "ok":
      lines.append("Busca indisponível: " + search["query"] + " [" + search.get("error", "failed") + "]")
  return "\n".join(lines)
