import { useEffect, useMemo, useRef, useState } from "react";
import { Check, ChevronDown, Search } from "lucide-react";

import "../../styles/components/searchableSelect.css";

function normalizarTexto(value) {
  return String(value ?? "")
    .normalize("NFD")
    .replace(/\p{Diacritic}/gu, "")
    .toLowerCase()
    .trim();
}

export default function SearchableSelect({
  id,
  value = "",
  options = [],
  placeholder = "Seleccione una opción",
  required = false,
  disabled = false,
  error = "",
  onChange,
}) {
  const containerRef = useRef(null);

  const [abierto, setAbierto] = useState(false);
  const [busqueda, setBusqueda] = useState("");
  const [indiceActivo, setIndiceActivo] = useState(0);

  const opcionSeleccionada = useMemo(() => options.find((option) => option.value === value) ?? null, [options, value]);

  const etiquetaSeleccionada = opcionSeleccionada?.label ?? "";

  const opcionesFiltradas = useMemo(() => {
    const busquedaNormalizada = normalizarTexto(busqueda);

    if (!busquedaNormalizada || busqueda === etiquetaSeleccionada) {
      return options;
    }

    return options.filter((option) => normalizarTexto(option.label).includes(busquedaNormalizada));
  }, [busqueda, etiquetaSeleccionada, options]);

  useEffect(() => {
    const handleClickExterior = (event) => {
      if (!containerRef.current?.contains(event.target)) {
        setAbierto(false);
      }
    };

    document.addEventListener("mousedown", handleClickExterior);

    return () => {
      document.removeEventListener("mousedown", handleClickExterior);
    };
  }, []);

  const abrirListado = () => {
    if (disabled) return;

    setBusqueda(etiquetaSeleccionada);
    setIndiceActivo(0);
    setAbierto(true);
  };

  const seleccionarOpcion = (option) => {
    onChange?.(option.value);
    setBusqueda(option.label);
    setAbierto(false);
  };

  const handleInputChange = (event) => {
    setBusqueda(event.target.value);
    setIndiceActivo(0);
    setAbierto(true);

    if (value) {
      onChange?.("");
    }
  };

  const handleKeyDown = (event) => {
    if (disabled) return;

    if (event.key === "ArrowDown") {
      event.preventDefault();
      setAbierto(true);
      setIndiceActivo((current) => Math.min(current + 1, opcionesFiltradas.length - 1));
    }

    if (event.key === "ArrowUp") {
      event.preventDefault();
      setIndiceActivo((current) => Math.max(current - 1, 0));
    }

    if (event.key === "Enter" && abierto && opcionesFiltradas[indiceActivo]) {
      event.preventDefault();
      seleccionarOpcion(opcionesFiltradas[indiceActivo]);
    }

    if (event.key === "Escape") {
      setAbierto(false);
    }
  };

  const inputValue = abierto ? busqueda : etiquetaSeleccionada;
  const placeholderCompleto = `${placeholder}${required ? "*" : ""}`;
  const listboxId = `${id}-options`;

  return (
    <div
      ref={containerRef}
      className={["searchable-select-field", abierto ? "open" : "", error ? "has-error" : "", disabled ? "disabled" : ""]
        .filter(Boolean)
        .join(" ")}
    >
      <div className="searchable-select-control">
        <input
          id={id}
          type="text"
          role="combobox"
          value={inputValue}
          placeholder={placeholderCompleto}
          disabled={disabled}
          autoComplete="off"
          aria-expanded={abierto}
          aria-controls={listboxId}
          aria-autocomplete="list"
          aria-invalid={Boolean(error)}
          onFocus={abrirListado}
          onChange={handleInputChange}
          onKeyDown={handleKeyDown}
        />

        <span className="searchable-select-search-icon">
          <Search size={17} />
        </span>

        <button
          type="button"
          className="searchable-select-toggle"
          tabIndex={-1}
          disabled={disabled}
          aria-label="Mostrar opciones"
          onClick={() => {
            if (abierto) {
              setAbierto(false);
            } else {
              abrirListado();
            }
          }}
        >
          <ChevronDown size={17} />
        </button>
      </div>

      {abierto && (
        <div id={listboxId} className="searchable-select-options" role="listbox">
          {opcionesFiltradas.length > 0 ? (
            opcionesFiltradas.map((option, index) => {
              const seleccionada = option.value === value;
              const activa = index === indiceActivo;

              return (
                <button
                  type="button"
                  role="option"
                  aria-selected={seleccionada}
                  key={option.value}
                  className={["searchable-select-option", seleccionada ? "selected" : "", activa ? "active" : ""].filter(Boolean).join(" ")}
                  onMouseDown={(event) => event.preventDefault()}
                  onClick={() => seleccionarOpcion(option)}
                >
                  <span>{option.label}</span>

                  {seleccionada && <Check size={16} />}
                </button>
              );
            })
          ) : (
            <div className="searchable-select-empty">No se encontraron opciones</div>
          )}
        </div>
      )}

      {error && <span className="searchable-select-error">{error}</span>}
    </div>
  );
}
