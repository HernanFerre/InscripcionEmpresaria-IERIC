import { useEffect, useLayoutEffect, useMemo, useRef, useState } from "react";

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
  label = "",
  value = "",
  options = [],
  placeholder = "Seleccione una opción",
  required = false,
  disabled = false,
  error = "",
  onChange,
}) {
  const containerRef = useRef(null);
  const listadoRef = useRef(null);
  const opcionActivaRef = useRef(null);

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

  const indiceVisible = Math.max(0, Math.min(indiceActivo, opcionesFiltradas.length - 1));

  useLayoutEffect(() => {
    if (!abierto) return;

    const listado = listadoRef.current;
    const opcionActiva = opcionActivaRef.current;

    if (!listado || !opcionActiva) return;

    const inicioOpcion = opcionActiva.offsetTop;
    const finOpcion = inicioOpcion + opcionActiva.offsetHeight;

    const inicioVisible = listado.scrollTop;
    const finVisible = inicioVisible + listado.clientHeight;

    if (inicioOpcion < inicioVisible) {
      listado.scrollTop = inicioOpcion;
    } else if (finOpcion > finVisible) {
      listado.scrollTop = finOpcion - listado.clientHeight;
    }
  }, [abierto, indiceVisible, opcionesFiltradas]);

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

    const indiceSeleccionado = options.findIndex((option) => option.value === value);

    setIndiceActivo(Math.max(0, indiceSeleccionado));
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

    if (event.key === "ArrowDown" || event.key === "ArrowUp") {
      event.preventDefault();

      if (!abierto) {
        abrirListado();
        return;
      }

      if (opcionesFiltradas.length > 0) {
        const desplazamiento = event.key === "ArrowDown" ? 1 : -1;

        setIndiceActivo(Math.max(0, Math.min(indiceVisible + desplazamiento, opcionesFiltradas.length - 1)));
      }

      return;
    }

    if (event.key === "Enter" && abierto) {
      event.preventDefault();

      if (opcionesFiltradas[indiceVisible]) {
        seleccionarOpcion(opcionesFiltradas[indiceVisible]);
      }

      return;
    }

    if (event.key === "Escape") {
      if (abierto) event.preventDefault();
      setAbierto(false);
    }

    if (event.key === "Tab") {
      setAbierto(false);
    }
  };

  const inputValue = abierto ? busqueda : etiquetaSeleccionada;
  const placeholderCompleto = placeholder;
  const listboxId = `${id}-options`;

  return (
    <div
      ref={containerRef}
      onBlur={(event) => {
        if (!event.currentTarget.contains(event.relatedTarget)) {
          setAbierto(false);
        }
      }}
      className={["searchable-select-field", abierto ? "open" : "", error ? "has-error" : "", disabled ? "disabled" : ""]
        .filter(Boolean)
        .join(" ")}
    >
      {label && (
        <label className="form-field-label" htmlFor={id}>
          {label}
          {required && <span aria-hidden="true">*</span>}
        </label>
      )}

      <div className="searchable-select-control">
        <span className="searchable-select-search-icon">
          <Search size={17} />
        </span>

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
          aria-activedescendant={abierto && opcionesFiltradas[indiceVisible] ? `${id}-option-${indiceVisible}` : undefined}
          aria-autocomplete="list"
          aria-invalid={Boolean(error)}
          onFocus={abrirListado}
          onChange={handleInputChange}
          onKeyDown={handleKeyDown}
          required={required}
        />

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
        <div ref={listadoRef} id={listboxId} className="searchable-select-options" role="listbox" aria-label={label || placeholder}>
          {opcionesFiltradas.length > 0 ? (
            opcionesFiltradas.map((option, index) => {
              const seleccionada = option.value === value;
              const activa = index === indiceVisible;

              return (
                <button
                  ref={activa ? opcionActivaRef : null}
                  id={`${id}-option-${index}`}
                  type="button"
                  role="option"
                  tabIndex={-1}
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
