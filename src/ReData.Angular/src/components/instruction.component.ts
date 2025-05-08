
import {Component, computed, effect, inject, input, model, viewChild} from '@angular/core';
import {EditorComponent} from 'ngx-monaco-editor-v2';
import {FormsModule} from '@angular/forms';
import {NzListModule} from 'ng-zorro-antd/list';
import {NzCardModule} from 'ng-zorro-antd/card';
import {NzInputModule} from 'ng-zorro-antd/input';
import {NzIconModule} from 'ng-zorro-antd/icon';
import {NzTypographyModule} from 'ng-zorro-antd/typography';

@Component({
  selector: 'app-instruction',
  standalone: true,
  imports: [
    EditorComponent,
    FormsModule,
    NzListModule,
    NzCardModule,
    NzInputModule,
    NzIconModule,
    NzTypographyModule,
  ],
  styles: `
    code {
      background-color: #f8f9fa;
      padding: 2px 4px;
      border-radius: 4px;
      font-family: monospace;
    }
    .example {
      background-color: #f0f7ff;
      padding: 10px 15px;
      border-left: 3px solid #3498db;
      margin: 10px 0;
    }
    .note {
      background-color: #fff8e1;
      padding: 10px 15px;
      border-left: 3px solid #ffc107;
      margin: 10px 0;
    }
  `,
  template: `
    <article nz-typography class="pb-10">
    <h2>Основные элементы языка</h2>

    <h3>Идентификаторы (имена столбцов и переменных)</h3>
    <ul>
      <li>Простые имена: <code>column1</code>, <code>column2</code>, <code>total_sum</code></li>
      <li>Имена с специальными символами (заключаются в квадратные скобки): <code>[column 2]</code>, <code>[2023 Sales]</code></li>
    </ul>

    <h3>Литералы</h3>
    <ul>
      <li><strong>Целые числа</strong>: <code>1</code>, <code>2</code>, <code>199</code>, <code>-45</code></li>
      <li><strong>Дробные числа</strong>: <code>123.45</code>, <code>-0.5</code> (всегда с точкой)</li>
      <li><strong>Текстовые строки</strong>: <code>'Привет'</code>, <code>'Hello world'</code> (в одинарных кавычках)</li>
    </ul>

    <h3>Операции</h3>
    <ul>
      <li><strong>Бинарные</strong>:
        <ul>
          <li>Арифметические: <code>+</code>, <code>-</code>, <code>*</code>, <code>/</code>, <code>^</code></li>
          <li>Сравнения: <code>&lt;</code>, <code>&gt;</code>, <code>&lt;=</code>, <code>&gt;=</code>, <code>=</code>, <code>!=</code></li>
          <li>Логические: <code>and</code>, <code>or</code></li>
        </ul>
      </li>
      <li><strong>Унарные</strong>:
        <ul>
          <li>Отрицание: <code>-5</code>, <code>-(a + b)</code></li>
        </ul>
      </li>
    </ul>

    <h3>Группировка выражений</h3>
    <p>Круглые скобки: <code>(a + b)</code></p>

    <h2>Функции</h2>

    <h3>Обычный синтаксис</h3>
    <p><code>ИмяФункции(аргумент1, аргумент2, ...)</code></p>

    <div class="example">
      <p><strong>Примеры:</strong></p>
      <ul>
        <li><code>Floor(column1)</code></li>
        <li><code>Date('2025-05-08')</code></li>
        <li><code>Date(2025, 5, 8)</code></li>
        <li><code>Pi()</code></li>
        <li><code>AddMonths(MyDate, 2)</code></li>
      </ul>
    </div>

    <h3>Методы объектов (альтернативный синтаксис)</h3>
    <p><code>аргумент1.ИмяФункции(аргумент2, ...)</code></p>

    <div class="example">
      <p><strong>Примеры:</strong></p>
      <ul>
        <li><code>column1.Floor()</code></li>
        <li><code>'2025-05-08'.Date()</code></li>
        <li><code>MyDate.AddMonths(2)</code></li>
        <li><code>[Дата создания].Year()</code></li>
      </ul>
    </div>

    <h2>Примеры выражений</h2>
    <div class="example">
      <ol>
        <li><strong>Простые вычисления:</strong>
          <ul>
            <li><code>(column1 + column2) / 2</code> - среднее двух столбцов</li>
            <li><code>column1 * 1.2</code> - увеличение на 20%</li>
          </ul>
        </li>
        <li><strong>Сравнения:</strong>
          <ul>
            <li><code>column1 > 100</code> - значения больше 100</li>
            <li><code>column1 = 'Москва' and column2 > 1000</code></li>
          </ul>
        </li>
        <li><strong>Работа с датами:</strong>
          <ul>
            <li><code>Date(2025, 5, 8) > Date('2025-01-01')</code> - сравнение дат</li>
            <li><code>Today().AddDays(7)</code> - дата через неделю</li>
          </ul>
        </li>
        <li><strong>Строковые операции:</strong>
          <ul>
            <li><code>[Имя] + ' ' + [Фамилия]</code> - объединение строк</li>
            <li><code>Reverse('Пример')</code> - переворачивание строки</li>
          </ul>
        </li>
        <li><strong>Альтернативный синтаксис позволяет создавать более читаемые цепочки обработки данных</strong>
          <ul>
            <li><code>IsNull(EmptyIsNull(Substring([Description], 2, 5)))</code> - стандартный</li>
            <li><code>[Description].Substring(2, 5).EmptyIsNull().IsNull()</code> - альтренативный</li>
          </ul>
        </li>
      </ol>
    </div>

    <h2>Особенности</h2>
    <div class="note">
      <ol>
        <li>Оба синтаксиса функций взаимозаменяемы:
          <ul>
            <li><code>Text(id)</code> эквивалентно <code>id.Text()</code></li>
          </ul>
        </li>
        <li>Для имён столбцов с пробелами или спецсимволами всегда используйте квадратные скобки: <code>[2023 Продажи]</code></li>
        <li>Литерала даты нет и их можно создать только с помощью функций:
          <ul>
            <li>Из строки: <code>Date('2025-05-08')</code></li>
            <li>Из чисел: <code>Date(2025, 5, 8)</code></li>
          </ul>
        </li>
        <li>Логические выражения можно комбинировать:
          <ul>
            <li><code>(a > 10 or b < 5) and c = 'active'</code></li>
          </ul>
        </li>
      </ol>
    </div>
    </article>
  `,
})
export class InstructionComponent {

}
