using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoHQApi.WebApi.Constantes
{
	public static class Constantes
	{
		private const string DIRETORIO_IMAGENS = @"D:\projetohqrepositorio\Capas\";
		private const string DIRETORIO_IMAGENS_AZURE = @"C:\projetohqrepositorio\Capas\";
		private const string DIRETORIO_IMAGENS_FRASES = @"D:\projetohqrepositorio\Frases\";

		private const string FORMATO_IMAGEM = @".jpg";


		public static string DIRETORIO_IMAGENS1 => DIRETORIO_IMAGENS;

		public static string GetFORMATO_IMAGEM()
		{
			return FORMATO_IMAGEM;
		}

		public static string GetDIRETORIO_IMAGENS()
		{
			return DIRETORIO_IMAGENS;
		}

		public static string GetDIRETORIO_IMAGENS_FRASES()
		{
			return DIRETORIO_IMAGENS_FRASES;
		}
	}
}
