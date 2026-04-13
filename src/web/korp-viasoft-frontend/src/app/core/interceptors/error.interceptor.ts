import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let mensagem = 'Erro inesperado. Tente novamente.';

      if (error.status === 0) {
        mensagem = 'Não foi possível conectar ao servidor.';
      } else if (error.status === 400) {
        mensagem = error.error?.detail || 'Dados inválidos.';
      } else if (error.status === 404) {
        mensagem = error.error?.detail || 'Recurso não encontrado.';
      } else if (error.status === 409) {
        mensagem = error.error?.detail || 'Conflito: operação não permitida.';
      } else if (error.status === 503) {
        mensagem = 'Serviço de estoque indisponível. Tente novamente em instantes.';
      } else if (error.status === 500) {
        mensagem = 'Erro interno no servidor.';
      }

      return throwError(() => ({ status: error.status, mensagem }));
    })
  );
};